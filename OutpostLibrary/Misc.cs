using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace OutpostLibrary
{
    public class IntVector3 : IEquatable<IntVector3>
    {
        public IntVector3(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        /// <summary>
        /// Creates a new IntVector3 with the specified value for each of its values.
        /// </summary>
        /// <param name="square"></param>
        public IntVector3(int square)
        {
            X = square;
            Y = square;
            Z = square;
        }

        public IntVector3(IntVector3 clone)
        {
            X = clone.X;
            Y = clone.Y;
            Z = clone.Z;
        }

        public int X;
        public int Y;
        public int Z;

        public override string ToString()
        {
            return X + "," + Y + "," + Z;
        }

        /// <summary>
        /// Returns a Vector3 which is a normalized version of the IntVector3
        /// </summary>
        /// <returns></returns>
        public Vector3 getNormalized()
        {
            Vector3 normal = new Vector3(X, Y, Z);
            normal.Normalize();
            return normal;
        }

        public static IntVector3 operator +(IntVector3 one, IntVector3 two)
        {
            return new IntVector3(one.X + two.X, one.Y + two.Y, one.Z + two.Z);
        }

        public static Vector3 operator +(IntVector3 one, Vector3 two)
        {
            return new Vector3(one.X + two.X, one.Y + two.Y, one.Z + two.Z);
        }

        public static Vector3 operator +(Vector3 one, IntVector3 two)
        {
            return new Vector3(one.X + two.X, one.Y + two.Y, one.Z + two.Z);
        }

        public static IntVector3 operator -(IntVector3 one, IntVector3 two)
        {
            return new IntVector3(one.X - two.X, one.Y - two.Y, one.Z - two.Z);
        }

        public static Vector3 operator -(Vector3 one, IntVector3 two)
        {
            return new Vector3(one.X - two.X, one.Y - two.Y, one.Z - two.Z);
        }

        public static Vector3 operator -(IntVector3 one, Vector3 two)
        {
            return new Vector3(one.X - two.X, one.Y - two.Y, one.Z - two.Z);
        }

        public static IntVector3 operator *(IntVector3 one, int two)
        {
            return new IntVector3(one.X * two, one.Y * two, one.Z * two);
        }

        public static IntVector3 operator *(int one, IntVector3 two)
        {
            return new IntVector3(one * two.X, one * two.Y, one * two.Z);
        }

        public static Vector3 operator *(IntVector3 one, float two)
        {
            return new Vector3(one.X * two, one.Y * two, one.Z * two);
        }

        public static Vector3 operator *(float one, IntVector3 two)
        {
            return new Vector3(one * two.X, one * two.Y, one * two.Z);
        }

        public static IntVector3 operator /(IntVector3 one, int two)
        {
            return new IntVector3(one.X / two, one.Y / two, one.Z / two);
        }

        public static Vector3 operator /(IntVector3 one, float two)
        {
            return new Vector3(one.X / two, one.Y / two, one.Z / two);
        }

        public static explicit operator Vector3(IntVector3 from)
        {
            return new Vector3(from.X, from.Y, from.Z);
        }

        /*/// <summary>
        /// Returns true if at least one value of the first is strictly greater than the corresponding value of the second,
        /// and each other value of the first is greater than or equal to the second.
        /// Returns false otherwise.
        /// (See also: >= and >>)
        /// ...Well I don't know what this would be used for anyway, so... *sigh*
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        public static bool operator >(IntVector3 one, IntVector3 two)
        {
            if (one.x < two.x || one.y < two.y || one.z < two.z)
                return false;
            if (one.x == two.x && one.y == two.y && one.z == two.z)
                return false;
            return true;
        }*/

        /// <summary>
        /// Returns true if each value of the first is greater than or equal to the corresponding value of the second, false otherwise.
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        public static bool operator >=(IntVector3 one, IntVector3 two)
        {
            if (one.X < two.X || one.Y < two.Y || one.Z < two.Z)
                return false;
            return true;
        }

        /// <summary>
        /// Returns true if each value of the first is strictly greater than the corresponding value of the second, false otherwise.
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        public static bool operator >(IntVector3 one, IntVector3 two)
        {
            if (one.X <= two.X || one.Y <= two.Y || one.Z <= two.Z)
                return false;
            return true;
        }

        /*
        public static bool operator <(IntVector3 one, IntVector3 two)
        {
            if (one.x > two.x || one.y > two.y || one.z > two.z)
                return false;
            if (one.x == two.x && one.y == two.y && one.z == two.z)
                return false;
            return true;
        }*/

        /// <summary>
        /// Returns true if each value of the first is less than or equal to the corresponding value of the second, false otherwise.
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        public static bool operator <=(IntVector3 one, IntVector3 two)
        {
            if (one.X > two.X || one.Y > two.Y || one.Z > two.Z)
                return false;
            return true;
        }

        /// <summary>
        /// Returns true if each value of the first is strictly less than the corresponding value of the second, false otherwise
        /// </summary>
        /// <param name="one"></param>
        /// <param name="two"></param>
        /// <returns></returns>
        public static bool operator <(IntVector3 one, IntVector3 two)
        {
            if (one.X >= two.X || one.Y >= two.Y || one.Z >= two.Z)
                return false;
            return true;
        }

        public bool Equals(IntVector3 other)
        {
            if (X != other.X || Y != other.Y || Z != other.Z)
                return false;
            return true;
        }
    }

    public struct IntVector2
    {
        public IntVector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int x;
        public int y;

        public override string ToString()
        {
            return x + "," + y;
        }

        public static IntVector2 operator +(IntVector2 one, IntVector2 two)
        {
            return new IntVector2(one.x + two.x, one.y + two.y);
        }

        public static IntVector2 operator -(IntVector2 one, IntVector2 two)
        {
            return new IntVector2(one.x - two.x, one.y - two.y);
        }

        public static bool operator >(IntVector2 one, IntVector2 two)
        {
            if (one.x <= two.x || one.y <= two.y)
                return false;
            return true;
        }

        public static bool operator >=(IntVector2 one, IntVector2 two)
        {
            if (one.x < two.x || one.y < two.y)
                return false;
            return true;
        }

        public static bool operator <(IntVector2 one, IntVector2 two)
        {
            if (one.x >= two.x || one.y >= two.y)
                return false;
            return true;
        }

        public static bool operator <=(IntVector2 one, IntVector2 two)
        {
            if (one.x > two.x || one.y > two.y)
                return false;
            return true;
        }
    }

    /*
    struct Direction
    {
        public bool north;
        public bool down;
        public bool west;
    }
    //*/

    public static class Misc
    {
        //public 

        #region IntVector extensions

        /// <summary>
        /// Returns the item at the position pointed to by the IntVector
        /// </summary>
        /// <typeparam name="T">The type stored in the array</typeparam>
        /// <param name="arr">The array to retrieve from</param>
        /// <param name="index">The position to retrieve from</param>
        /// <returns>The retrieved item</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the index given is out of the range of the array given.</exception>
        public static T get<T>(this T[, ,] arr, IntVector3 index)
        {
            return arr[index.X, index.Y, index.Z];
        }

        /// <summary>
        /// Sets the position pointed to by the IntVector to the given item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public static void set<T>(this T[, ,] arr, IntVector3 index, T item)
        {
            arr[index.X, index.Y, index.Z] = item;
        }

        /// <summary>
        /// Returns the item at the position pointed to by the IntVector
        /// </summary>
        /// <typeparam name="T">The type stored in the array</typeparam>
        /// <param name="arr">The array to retrieve from</param>
        /// <param name="index">The position to retrieve from</param>
        /// <returns>The retrieved item</returns>
        public static T get<T>(this T[,] arr, IntVector2 index)
        {
            return arr[index.x, index.y];
        }

        /// <summary>
        /// Sets the position pointed to by the IntVector to the given item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr"></param>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public static void set<T>(this T[,] arr, IntVector2 index, T item)
        {
            arr[index.x, index.y] = item;
        }
        #endregion

        #region colorConversions

        public static System.Drawing.Color toSystemColor(this Microsoft.Xna.Framework.Color convertFrom)
        {
            return System.Drawing.Color.FromArgb(convertFrom.A, convertFrom.R, convertFrom.G, convertFrom.B);
        }

        public static Microsoft.Xna.Framework.Color toXnaColor(this System.Drawing.Color convertFrom)
        {
            return new Microsoft.Xna.Framework.Color(convertFrom.R, convertFrom.G, convertFrom.B, convertFrom.A);
        }

        #endregion colorConversions
    }
}
