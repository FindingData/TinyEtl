///////////////////////////////////////////////////////////
//  TinyGuard.cs
//  Implementation of the Class TinyGuard
//  Generated by Enterprise Architect
//  Created on:      06-5��-2018 18:08:41
//  Original author: drago
///////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



namespace FD.TinyEtl {
	/// <summary>
	/// <font color="#008000"> Represents a simple class for validating parameters and
	/// throwing exceptions.</font>
	/// </summary>
	public static class TinyGuard {
        public static void NotNull(object argumentValue,string argumentName)
        {
            if (!IsNotNull(argumentValue))
            {
                throw new NullReferenceException(argumentName);
            }
        }

        private static bool IsNotNull(object argumentValue)
        {
            return argumentValue != null;
        }

        // Summary:
        //     Validates argumentValue is not null and throws System.ArgumentNullException
        //     if it is null.
        //
        // Parameters:
        //   argumentValue:
        //     The value to validate.
        //
        //   argumentName:
        //     The name of argumentValue.
        public static void ArgumentNotNull(object argumentValue, string argumentName)
        {
            if (!IsArgumentNotNull(argumentValue))
                throw new ArgumentNullException(argumentName);
        }

        public static bool IsArgumentNotNull(object argumentValue)
        {
            return IsNotNull(argumentValue);
        }

        public static void ArgumentNotDbNull(object argumentValue, string argumentName)
        {
            if (!IsArgumentNotDbNull(argumentValue))
                throw new ArgumentNullException(argumentName);
        }

        public static bool IsArgumentNotDbNull(object argumentValue)
        {
            return argumentValue != null && argumentValue != DBNull.Value;
        }

        public static void ArgumentNotNullOrEmpty(object argumentValue, string argumentName)
        {
            if (!IsArgumentNotNullOrEmpty(argumentValue))
                throw new ArgumentNullException(argumentName);
        }

        public static bool IsArgumentNotNullOrEmpty(object argumentValue)
        {
            if (!IsNotNull(argumentValue)) return false;

            if (argumentValue is string && String.IsNullOrEmpty((String)argumentValue))
                return false;

            if (argumentValue is ICollection && ((ICollection)argumentValue).Count == 0)
                return false;

            return true;
        }

        //
        // Summary:
        //     Validates argumentValue is not null or an empty string and throws System.ArgumentNullException
        //     if it is null or an empty string .
        //
        // Parameters:
        //   argumentValue:
        //     The value to validate.
        //
        //   argumentName:
        //     The name of argumentValue.
        public static void ArgumentNotNullOrEmpty(string argumentValue, string argumentName)
        {
            if (!IsArgumentNotNullOrEmpty(argumentValue))
                throw new ArgumentNullException(argumentName);
        }

        public static bool IsArgumentNotNullOrEmpty(string argumentValue)
        {
            if (String.IsNullOrEmpty(argumentValue))
                return false;
            else
                return true;
        }

        public static void NotNullOrEmpty(string argumentValue, string argumentName)
        {
            if (!IsNotNullOrEmpty(argumentValue))
                throw new NullReferenceException(argumentName);
        }

        public static bool IsNotNullOrEmpty(string argumentValue)
        {
            if (String.IsNullOrEmpty(argumentValue))
                return false;
            else
                return true;
        }
    }//end TinyGuard

}//end namespace FD.TinyEtl