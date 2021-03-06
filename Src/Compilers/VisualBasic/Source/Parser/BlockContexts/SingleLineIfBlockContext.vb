﻿' Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
'-----------------------------------------------------------------------------
' Contains the definition of the BlockContext
'-----------------------------------------------------------------------------

Namespace Microsoft.CodeAnalysis.VisualBasic.Syntax.InternalSyntax

    Friend NotInheritable Class SingleLineIfBlockContext
        Inherits SingleLineIfOrElseBlockContext

        Private _optionalElsePart As SingleLineElsePartSyntax

        Friend Sub New(statement As StatementSyntax, prevContext As BlockContext)
            MyBase.New(SyntaxKind.SingleLineIfStatement, statement, prevContext)

            Debug.Assert(statement.Kind = SyntaxKind.IfStatement)
            Debug.Assert(DirectCast(statement, IfStatementSyntax).ThenKeyword IsNot Nothing)
        End Sub

        Friend Overrides Function ProcessSyntax(node As VisualBasicSyntaxNode) As BlockContext
            Select Case node.Kind
                Case SyntaxKind.IfStatement
                    Dim ifStmt = DirectCast(node, IfStatementSyntax)
                    ' A single line if has a "then" on the line and is not followed by a ":", EOL or EOF.
                    ' It is OK for else to follow a single line if. i.e
                    '       "if true then if true then else else
                    If ifStmt.ThenKeyword IsNot Nothing AndAlso Not SyntaxFacts.IsTerminator(Parser.CurrentToken.Kind) Then
                        Return New SingleLineIfBlockContext(ifStmt, Me)
                    End If

                Case SyntaxKind.ElseIfStatement
                    Dim elseIfStmt = DirectCast(node, IfStatementSyntax)

                    If elseIfStmt.ElseKeyword IsNot Nothing Then
                        ' The parser always parses "else if" as an elseif statement.
                        ' Decompose this into an else statement and an if statement.
                        Dim elseStmt = SyntaxFactory.ElseStatement(elseIfStmt.ElseKeyword)
                        Dim ifStmt = SyntaxFactory.IfStatement(Nothing, elseIfStmt.IfOrElseIfKeyword, elseIfStmt.Condition, elseIfStmt.ThenKeyword)

                        ' Create else context
                        Dim context = New SingleLineElseContext(SyntaxKind.SingleLineElsePart, elseStmt, Me)

                        ' Let the else context process the if.
                        Return context.ProcessSyntax(ifStmt)
                    Else
                        'ElseIf is unsupported in line if. End the line if and report expected end of statement per Dev10
                        Add(Parser.ReportSyntaxError(node, ERRID.ERR_ExpectedEOS))
                        Return Me.EndBlock(Nothing)
                    End If

                Case SyntaxKind.ElseStatement
                    Return New SingleLineElseContext(SyntaxKind.SingleLineElsePart, DirectCast(node, StatementSyntax), Me)

                Case SyntaxKind.SingleLineElsePart
                    _optionalElsePart = DirectCast(node, SingleLineElsePartSyntax)
                    Return Me

                Case SyntaxKind.CatchStatement, SyntaxKind.FinallyStatement
                    ' A Catch or Finally always closes a single line if
                    Add(Parser.ReportSyntaxError(node, If(node.Kind = SyntaxKind.CatchStatement, ERRID.ERR_CatchNoMatchingTry, ERRID.ERR_FinallyNoMatchingTry)))
                    Return Me.EndBlock(Nothing)

            End Select

            Return MyBase.ProcessSyntax(node)
        End Function

        Friend Overrides Function CreateBlockSyntax(endStmt As StatementSyntax) As VisualBasicSyntaxNode
            Debug.Assert(endStmt Is Nothing)
            Return CreateIfBlockSyntax()
        End Function

        Private Function CreateIfBlockSyntax() As SingleLineIfStatementSyntax
            Debug.Assert(BeginStatement IsNot Nothing)

            Dim result = SyntaxFactory.SingleLineIfStatement(SyntaxFactory.SingleLineIfPart(DirectCast(BeginStatement, IfStatementSyntax), Body()), _optionalElsePart)

            FreeStatements()

            Return result
        End Function

        Friend Overrides Function EndBlock(statement As StatementSyntax) As BlockContext
            Debug.Assert(statement Is Nothing)
            Dim blockSyntax = CreateIfBlockSyntax()
            Return PrevBlock.ProcessSyntax(blockSyntax)
        End Function

        Friend Overrides Function ProcessStatementTerminator(lambdaContext As BlockContext) As BlockContext
            Dim token = Parser.CurrentToken
            Select Case token.Kind
                Case SyntaxKind.StatementTerminatorToken, SyntaxKind.EndOfFileToken
                    ' A single-line If is terminated at the end of the line.
                    Dim context = EndBlock(Nothing)
                    Return context.ProcessStatementTerminator(lambdaContext)

                Case SyntaxKind.ColonToken
                    ' A colon does not represent the end of the single-line if.
                    Debug.Assert(_statements.Count > 0)
                    Parser.ConsumeColonInSingleLineExpression()
                    Return Me

                Case SyntaxKind.ElseKeyword
                    Parser.ConsumedStatementTerminator(allowLeadingMultilineTrivia:=False)
                    Return Me

                Case Else
                    ' Terminated if we've already seen at least one statement.
                    If _statements.Count > 0 Then
                        Return ProcessOtherAsStatementTerminator()
                    End If
                    Parser.ConsumedStatementTerminator(allowLeadingMultilineTrivia:=False)
                    Return Me

            End Select
        End Function

        Friend Overrides Function ProcessOtherAsStatementTerminator() As BlockContext
            Dim context = EndBlock(Nothing)
            Return context.ProcessOtherAsStatementTerminator()
        End Function

    End Class

End Namespace
