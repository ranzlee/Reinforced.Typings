﻿using System;
using System.Reflection;
using Reinforced.Typings.Ast;
using Reinforced.Typings.Attributes;

namespace Reinforced.Typings.Generators
{
    /// <summary>
    ///     Default code generator for properties
    /// </summary>
    public class PropertyCodeGenerator : ITsCodeGenerator<MemberInfo, RtField>
    {
        /// <summary>
        ///     Main code generator method. This method should write corresponding TypeScript code for element (1st argument) to
        ///     WriterWrapper (3rd argument) using TypeResolver if necessary
        /// </summary>
        /// <param name="element">Element code to be generated to output</param>
        /// <param name="resolver">Type resolver</param>
        public virtual RtField Generate(MemberInfo element, TypeResolver resolver)
        {
            if (element.IsIgnored()) return null;
            RtField result = new RtField();

            var doc = Settings.Documentation.GetDocumentationMember(element);
            if (doc != null)
            {
                RtJsdocNode jsdoc = new RtJsdocNode();
                jsdoc.Description = doc.Summary.Text;
            }

            var t = GetType(element);
            RtTypeName type = null;
            var propName = new RtIdentifier(element.Name);
            
            var tp = ConfigurationRepository.Instance.ForMember<TsPropertyAttribute>(element);
            if (tp != null)
            {
                if (tp.StrongType != null)
                {
                    type = resolver.ResolveTypeName(tp.StrongType);
                }
                else if (!string.IsNullOrEmpty(tp.Type))
                {
                    type = new RtSimpleTypeName(tp.Type);
                }

                if (!string.IsNullOrEmpty(tp.Name)) propName = new RtIdentifier(tp.Name);
                if (tp.ForceNullable && element.DeclaringType.IsExportingAsInterface() && !Settings.SpecialCase)
                    propName.IsNullable = true;
            }

            if (type == null) type = resolver.ResolveTypeName(t);
            if (!propName.IsNullable && t.IsNullable() && element.DeclaringType.IsExportingAsInterface() &&
                !Settings.SpecialCase)
            {
                result.Identifier.IsNullable = true;
            }

            if (element is PropertyInfo)
            {
                propName.IdentifierName = Settings.ConditionallyConvertPropertyNameToCamelCase(propName.IdentifierName);
            }
            propName.IdentifierName = element.CamelCaseFromAttribute(propName.IdentifierName);

            result.AccessModifier = Settings.SpecialCase ? AccessModifier.Public : element.GetModifier();
            result.Type = type;

            return result;
        }

        /// <summary>
        ///     Export settings
        /// </summary>
        public ExportSettings Settings { get; set; }

        /// <summary>
        ///     Returns type of specified property. It is useful for overloads sometimes
        /// </summary>
        /// <param name="mi">Method Info</param>
        /// <returns>Property info type</returns>
        protected virtual Type GetType(MemberInfo mi)
        {
            var pi = (PropertyInfo)mi;
            return pi.PropertyType;
        }
    }
}