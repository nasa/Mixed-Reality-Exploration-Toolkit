// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

using System;

namespace GOV.NASA.GSFC.XR.Utilities.Json
{
    /// <remarks>
    /// History:
    /// 19 June 2023: Created
    /// </remarks>
	///
	/// <summary>
	/// JsonException
	///
	/// JSON Exception classes
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
    public class JsonException : Exception
    {
        public JsonException(string message) : base(message)
        {
        }

        public JsonException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
