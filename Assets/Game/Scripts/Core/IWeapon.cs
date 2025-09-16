using UnityEngine;
using System;
// IWeapon.cs
public interface IWeapon { void TryHit(UnitStats owner, Transform preferredTarget = null); }
