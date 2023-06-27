// Copyright � 2018-2022 United States Government as represented by the Administrator
// of the National Aeronautics and Space Administration. All Rights Reserved.

namespace GOV.NASA.GSFC.XR.XRC
{
    /// <remarks>
    /// History:
    /// 21 December 2022: Created
    /// </remarks>
	///
	/// <summary>
	/// IRemoteSession
	///
	/// Represents a remote collaboration session
	///
    /// Author: Jeffrey Hosler
	/// </summary>
	/// 
	public interface IRemoteSession
	{
        public long NumUsers { get; }
        public string id { get; }
        public string Group { get; }
        public string Project { get; }
        public string Session { get; }
        public string Platform { get; }
    }
}
