using System;
using System.Runtime.Serialization;
using System.Text;

namespace ALinq
{
    /// <summary>
    /// Represents an immutable block of binary data.
    /// </summary>
    [Serializable, DataContract]
    public sealed class Binary : IEquatable<Binary>
    {
        // Fields

        [DataMember(Name = "Bytes")]
        private byte[] bytes;
        private int? hashCode;


        // Methods
        /// <summary>
        /// Initializes a new instance of the ALinq.Binary class.
        /// </summary>
        /// <param name="value">The bytes representing the binary data.</param>
        public Binary(byte[] value)
        {
            if (value == null)
            {
                throw Error.ArgumentNull("value");
            }
            bytes = new byte[value.Length];
            Array.Copy(value, bytes, value.Length);
            ComputeHash();
        }

        private void ComputeHash()
        {
            int num = 0x13a;
            int num2 = 0x9f;
            hashCode = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                //int? hashCode = this.hashCode;
                int num4 = num;
                int? nullable3 = hashCode.HasValue ? new int?(hashCode.GetValueOrDefault() * num4) : null;
                int num5 = this.bytes[i];
                this.hashCode = nullable3.HasValue ? new int?(nullable3.GetValueOrDefault() + num5) : null;
                num *= num2;
            }
        }

        /// <summary>
        /// Determines whether two binary objects are equal.
        /// </summary>
        /// <param name="other">The System.Object to which the current object is being compared.</param>
        /// <returns>true if the two binary objects are equal; otherwise, false.</returns>
        public bool Equals(Binary other)
        {
            return this.EqualsTo(other);
        }

        /// <summary>
        /// Determines whether the specified System.Object is equal to the current System.Object.
        /// </summary>
        /// <param name="obj">The System.Object to compare with the current System.Object.</param>
        /// <returns>true if the two binary objects are equal; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return this.EqualsTo(obj as Binary);
        }

        /// <summary>
        /// Determines whether two binary objects are equal.
        /// </summary>
        /// <param name="binary">The System.Object to which the current object is being compared.
        /// </param>
        /// <returns>true if the two binary objects are equal; otherwise, false.</returns>
        private bool EqualsTo(Binary binary)
        {
            if (this != binary)
            {
                if (binary == null)
                {
                    return false;
                }
                if (this.bytes.Length != binary.bytes.Length)
                {
                    return false;
                }
                if (this.hashCode != binary.hashCode)
                {
                    return false;
                }
                int index = 0;
                int length = this.bytes.Length;
                while (index < length)
                {
                    if (this.bytes[index] != binary.bytes[index])
                    {
                        return false;
                    }
                    index++;
                }
            }
            return true;
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current binary object.</returns>
        public override int GetHashCode()
        {
            if (!this.hashCode.HasValue)
            {
                this.ComputeHash();
            }
            return this.hashCode.Value;
        }

        /// <summary>
        /// Describes the equality relationship between two binary objects.
        /// </summary>
        /// <param name="binary1">First binary object.</param>
        /// <param name="binary2">Second binary object.</param>
        /// <returns>true if the binary objects are equal; otherwise false.</returns>
        public static bool operator ==(Binary binary1, Binary binary2)
        {
            //return ((binary1 == binary2) || (((binary1 == null) && (binary2 == null)) || (((binary1 != null) && (binary2 != null)) && binary1.EqualsTo(binary2))));
            if (ReferenceEquals(binary1, null) && ReferenceEquals(binary2, null))
                return true;
            if (!ReferenceEquals(binary1, null) && !ReferenceEquals(binary2, null))
                return ReferenceEquals(binary1, binary2);

            return false;
        }

        /// <summary>
        /// Enables arrays of bytes to be implicitly coerced to the ALinq.Binary type in a programming language.
        /// </summary>
        /// <param name="value"> The array of bytes to convert into an instance of the ALinq.Binary type.</param>
        /// <returns>A ALinq.Binary class containing the coerced value.</returns>
        public static implicit operator Binary(byte[] value)
        {
            return new Binary(value);
        }

        /// <summary>
        /// Describes the inequality relationship between two binary objects.
        /// </summary>
        /// <param name="binary1">The first binary object.</param>
        /// <param name="binary2">The second binary object.</param>
        /// <returns>true if the binary objects are not equal; otherwise false.</returns>
        public static bool operator !=(Binary binary1, Binary binary2)
        {
            if (binary1 == binary2)
            {
                return false;
            }
            if ((binary1 == null) && (binary2 == null))
            {
                return false;
            }
            if ((!(binary1 == null)) && (!(binary2 == null)))
            {
                return !binary1.EqualsTo(binary2);
            }
            return true;
        }

        /// <summary>
        /// Returns an array of bytes that represents the current binary object.
        /// </summary>
        /// <returns>A byte array that contains the value of the current binary object.</returns>
        public byte[] ToArray()
        {
            byte[] destinationArray = new byte[this.bytes.Length];
            Array.Copy(this.bytes, destinationArray, destinationArray.Length);
            return destinationArray;
        }

        /// <summary>
        /// Returns a System.String that represents the current binary object.
        /// </summary>
        /// <returns>A System.String that represents the current binary object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("\"");
            builder.Append(Convert.ToBase64String(this.bytes, 0, this.bytes.Length));
            builder.Append("\"");
            return builder.ToString();
        }

        /// <summary>
        /// Gets the length of the binary object.
        /// </summary>
        public int Length
        {
            get
            {
                return this.bytes.Length;
            }
        }
    }



}
