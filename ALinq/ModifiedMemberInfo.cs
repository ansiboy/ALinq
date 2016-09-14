using System.Reflection;
using System.Runtime.InteropServices;

namespace ALinq
{
    /// <summary>
    /// Holds values of members that have been modified in LINQ to SQL applications.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ModifiedMemberInfo
    {
        private readonly MemberInfo member;
        private readonly object current;
        private readonly object original;

        internal ModifiedMemberInfo(MemberInfo member, object current, object original)
        {
            this.member = member;
            this.current = current;
            this.original = original;
        }

        /// <summary>
        /// Gets member information for the modified member.
        /// </summary>
        /// <returns>
        /// Information about the member in conflict.
        /// </returns>
        public MemberInfo Member
        {
            get
            {
                return this.member;
            }
        }

        /// <summary>
        /// Gets the current value of the modified member.
        /// </summary>
        /// <returns>
        /// The value of the member.
        /// </returns>
        public object CurrentValue
        {
            get
            {
                return this.current;
            }
        }

        /// <summary>
        /// Gets the original value of the modified member.
        /// </summary>
        /// <returns>
        /// The original value for the modified member.
        /// </returns>
        public object OriginalValue
        {
            get
            {
                return this.original;
            }
        }
    }
}