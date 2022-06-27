using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class AssetAddress : IEquatable<AssetAddress>
{
    const string SEP = ":";

    public readonly string mod;
    public readonly string path;

    public string Mod => mod;
    public string Path => path;
    public string Address => Mod + SEP + Path;

    public AssetAddress(string path, string mod = "campain")
    {
        if (string.IsNullOrEmpty(path) || !Valid(path))
            throw new ArgumentException("Invalid Path for AssetAddress! Null or Invalid Characters!", "path");
        if (string.IsNullOrEmpty(mod) || !Valid(mod))
            throw new ArgumentException("Invalid Mod for AssetAddress! Null or Invalid Characters!", "mod");
    }

    public static bool Valid(char ch)
    {
        return ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'z' || ch == '_' || ch == ':' || ch == '/' || ch == '.' || ch == '-';
    }

    public static bool Valid(string str)
    {
        return str.ToCharArray().All(c => Valid(c));
    }

    #region Equality Checking
    public override string ToString()
    {
        return Address;
    }

    public override bool Equals(object obj) => Equals(obj as AssetAddress);

    public bool Equals(AssetAddress other)
    {
        if (ReferenceEquals(other, this))
            return true;
        if (ReferenceEquals(other, null))
            return false;
        return mod == other.mod && path == other.path;
    }

    public override int GetHashCode()
    {
        return mod.GetHashCode() ^ path.GetHashCode();
    }

    public static bool operator ==(AssetAddress lhs, AssetAddress rhs)
    {
        if (ReferenceEquals(lhs, rhs))
            return true;
        if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
            return false;
        return lhs.Equals(rhs);
    }

    public static bool operator !=(AssetAddress lhs, AssetAddress rhs)
    {
        if (ReferenceEquals(lhs, rhs))
            return false;
        if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
            return true;
        return !lhs.Equals(rhs);
    }
    #endregion
}
