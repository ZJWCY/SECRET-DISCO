using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    static public bool[] isMythUnlocked;
    static public HashSet<ushort> uncheckedMyths;
    static public HashSet<string> flags;
    static public HashSet<string> answeredDialogs;
    static public Dictionary<string, bool> isAnswerUnhidden;
    static public float San
    {
        get => san;
        set
        {
            if (GameManager.gameStage == 1)
                return;
            if (value <= 150f)
                san = value;
        }
    }
    static private float san;

    static public void LoadDatas()
    {
        san = 150f;
        isMythUnlocked = new bool[Myth.allMyths.Length];
        uncheckedMyths = new HashSet<ushort>();
        flags = new HashSet<string>();
        answeredDialogs = new HashSet<string>();
        isAnswerUnhidden = new Dictionary<string, bool>();
    }

    static public bool CheckMyth(string info)
    {
        if (info.StartsWith('!'))
        {
            if (isMythUnlocked[ushort.Parse(info.Remove(0, 1))])
                return false;
        }
        else
        {
            if (!isMythUnlocked[ushort.Parse(info)])
                return false;
        }
        return true;
    }

    static public bool CheckFlag(string info)
    {
        if (info.StartsWith('!'))
        {
            if (flags.Contains(info.Remove(0, 1)))
                return false;
        }
        else
        {
            if (!flags.Contains(info))
                return false;
        }
        return true;
    }

    static public bool CheckDialog(string info)
    {
        if (info.StartsWith('!'))
        {
            if (answeredDialogs.Contains(info.Remove(0, 1)))
                return false;
        }
        else
        {
            if (!answeredDialogs.Contains(info))
                return false;
        }
        return true;
    }
}
