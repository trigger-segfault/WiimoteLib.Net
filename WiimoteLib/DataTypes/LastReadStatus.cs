using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiimoteLib.DataTypes {
	/// <summary>
	/// Last ReadData status
	/// </summary>
	public enum LastReadStatus {
		/// <summary>
		/// Successful read
		/// </summary>
		Success,
		/// <summary>
		/// Attempt to read from write only memory
		/// </summary>
		ReadFromWriteOnlyMemory
	}
}
