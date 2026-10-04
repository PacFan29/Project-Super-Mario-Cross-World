using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CreditsLines", menuName = "ScriptableObjects/CreditsLines")]
public class CreditsLines : ScriptableObject
{
    [Serializable]
    public class CreditsItem
    {
        public string header;
        [TextArea] public string detail;
    }

    public CreditsItem[] items;
}
