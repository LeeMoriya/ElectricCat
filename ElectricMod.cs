using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Partiality.Modloader;
using UnityEngine;
using Menu;
using Music;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security;
using SlugBase;

//Remove PublicityStunt requirement
[assembly: IgnoresAccessChecksTo("Assembly-CSharp")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class IgnoresAccessChecksToAttribute : Attribute
    {
        public IgnoresAccessChecksToAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }
        public string AssemblyName { get; }
    }
}
public class ElectricMod : PartialityMod
{
    public ElectricMod()
    {
        this.ModID = "Electric Cat 2.0";
        this.Version = "2.0";
        this.author = "Shiro_pb + LeeMoriya";
    }

    public override void OnEnable()
    {
        PlayerManager.RegisterCharacter(new ElectricCat());
    }
}
