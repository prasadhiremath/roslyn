﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;
using Cci = Microsoft.Cci;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.CSharp.Symbols
{
    partial class PropertySymbol :
        Cci.IPropertyDefinition
    {
        #region IPropertyDefinition Members

        IEnumerable<Cci.IMethodReference> Cci.IPropertyDefinition.Accessors
        {
            get
            {
                CheckDefinitionInvariant();

                var getMethod = this.GetMethod;
                if ((object)getMethod != null)
                {
                    yield return getMethod;
                }

                var setMethod = this.SetMethod;
                if ((object)setMethod != null)
                {
                    yield return setMethod;
                }

                SourcePropertySymbol sourceProperty = this as SourcePropertySymbol;
                if ((object)sourceProperty != null)
                {
                    SynthesizedSealedPropertyAccessor synthesizedAccessor = sourceProperty.SynthesizedSealedAccessorOpt;
                    if ((object)synthesizedAccessor != null)
                    {
                        yield return synthesizedAccessor;
                    }
                }
            }
        }

        Cci.IMetadataConstant Cci.IPropertyDefinition.DefaultValue
        {
            get
            {
                CheckDefinitionInvariant();
                return null;
            }
        }

        Cci.IMethodReference Cci.IPropertyDefinition.Getter
        {
            get
            {
                CheckDefinitionInvariant();
                MethodSymbol getMethod = this.GetMethod;
                if ((object)getMethod != null || !this.IsSealed)
                {
                    return getMethod;
                }

                return GetSynthesizedSealedAccessor(MethodKind.PropertyGet);
            }
        }

        bool Cci.IPropertyDefinition.HasDefaultValue
        {
            get
            {
                CheckDefinitionInvariant();
                return false;
            }
        }

        bool Cci.IPropertyDefinition.IsRuntimeSpecial
        {
            get
            {
                CheckDefinitionInvariant();
                return HasRuntimeSpecialName;
            }
        }

        internal virtual bool HasRuntimeSpecialName
        {
            get
            {
                CheckDefinitionInvariant();
                return false;
            }
        }

        bool Cci.IPropertyDefinition.IsSpecialName
        {
            get
            {
                CheckDefinitionInvariant();
                return this.HasSpecialName;
            }
        }

        ImmutableArray<Cci.IParameterDefinition> Cci.IPropertyDefinition.Parameters
        {
            get
            {
                CheckDefinitionInvariant();
                return StaticCast<Cci.IParameterDefinition>.From(this.Parameters);
            }
        }

        Cci.IMethodReference Cci.IPropertyDefinition.Setter
        {
            get
            {
                CheckDefinitionInvariant();
                MethodSymbol setMethod = this.SetMethod;
                if ((object)setMethod != null || !this.IsSealed)
                {
                    return setMethod;
                }

                return GetSynthesizedSealedAccessor(MethodKind.PropertySet);
            }
        }

        #endregion

        #region ISignature Members

        Cci.CallingConvention Cci.ISignature.CallingConvention
        {
            get
            {
                CheckDefinitionInvariant();
                return this.CallingConvention;
            }
        }

        ushort Cci.ISignature.ParameterCount
        {
            get
            {
                CheckDefinitionInvariant();
                return (ushort)this.ParameterCount;
            }
        }

        ImmutableArray<Cci.IParameterTypeInformation> Cci.ISignature.GetParameters(Microsoft.CodeAnalysis.Emit.Context context)
        {
            CheckDefinitionInvariant();
            return StaticCast<Cci.IParameterTypeInformation>.From(this.Parameters);
        }

        IEnumerable<Cci.ICustomModifier> Cci.ISignature.ReturnValueCustomModifiers
        {
            get
            {
                CheckDefinitionInvariant();
                return this.TypeCustomModifiers;
            }
        }

        bool Cci.ISignature.ReturnValueIsByRef
        {
            get
            {
                CheckDefinitionInvariant();
                return this.Type is ByRefReturnErrorTypeSymbol;
            }
        }

        bool Cci.ISignature.ReturnValueIsModified
        {
            get
            {
                CheckDefinitionInvariant();
                return this.TypeCustomModifiers.Length != 0;
            }
        }

        Cci.ITypeReference Cci.ISignature.GetType(Microsoft.CodeAnalysis.Emit.Context context)
        {
            CheckDefinitionInvariant();
            return ((PEModuleBuilder)context.Module).Translate(this.Type,
                                                      syntaxNodeOpt: (CSharpSyntaxNode)context.SyntaxNodeOpt,
                                                      diagnostics: context.Diagnostics);
        }

        #endregion

        #region ITypeDefinitionMember Members

        Cci.ITypeDefinition Cci.ITypeDefinitionMember.ContainingTypeDefinition
        {
            get
            {
                CheckDefinitionInvariant();
                return this.ContainingType;
            }
        }

        Cci.TypeMemberVisibility Cci.ITypeDefinitionMember.Visibility
        {
            get
            {
                CheckDefinitionInvariant();
                return PEModuleBuilder.MemberVisibility(this);
            }
        }

        #endregion

        #region ITypeMemberReference Members

        Cci.ITypeReference Cci.ITypeMemberReference.GetContainingType(Microsoft.CodeAnalysis.Emit.Context context)
        {
            CheckDefinitionInvariant();
            return this.ContainingType;
        }

        #endregion

        #region IReference Members

        void Cci.IReference.Dispatch(Cci.MetadataVisitor visitor)
        {
            CheckDefinitionInvariant();
            visitor.Visit((Cci.IPropertyDefinition)this);
        }

        Cci.IDefinition Cci.IReference.AsDefinition(Microsoft.CodeAnalysis.Emit.Context context)
        {
            CheckDefinitionInvariant();
            return this;
        }

        #endregion

        #region INamedEntity Members

        string Cci.INamedEntity.Name
        {
            get
            {
                CheckDefinitionInvariant();
                return this.MetadataName;
            }
        }

        #endregion

        private Cci.IMethodReference GetSynthesizedSealedAccessor(MethodKind targetMethodKind)
        {
            SourcePropertySymbol sourceProperty = this as SourcePropertySymbol;
            if ((object)sourceProperty != null)
            {
                SynthesizedSealedPropertyAccessor synthesized = sourceProperty.SynthesizedSealedAccessorOpt;
                return (object)synthesized != null && synthesized.MethodKind == targetMethodKind ? synthesized : null;
            }

            return null;
        }
    }
}
