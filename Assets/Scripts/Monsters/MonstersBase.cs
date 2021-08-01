using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Monster", menuName = "Monster/Create new Monster")]
public class MonstersBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] MonsterType type1;
    [SerializeField] MonsterType type2;

    //Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableMove> learnableMoves;

    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }

    public Sprite BackSprite
    {
        get { return backSprite; }
    }

    public MonsterType Type1
    {
        get { return type1; }
    }

    public MonsterType Type2
    {
        get { return type2; }
    }

    public int MaxHp
    {
        get { return maxHp; }
    }

    public int Attack
    {
        get { return attack; }
    }

    public int Defense
    {
        get { return defense; }
    }

    public int SpAttack
    {
        get { return spAttack; }
    }

    public int SpDefense
    {
        get { return spDefense; }
    }

    public int Speed
    {
        get { return speed; }
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }
}

public enum MonsterType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon
}

public class TypeChart
{
	static float[][] chart =
	{
		//                           NOR   FIR   WAT   ELE   GRA   ICE   FIG   POI
		/*NORM*/        new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f},
		/*FIRE*/        new float[] {1f,   0.5f, 0.5f, 2f,   2f,   1f,   1f,   1f},
		/*WATER*/       new float[] {1f,   2f,  0.5f,  2f,   0.5f, 1f,   1f,   1f},
		/*ELEC*/        new float[] {1f,   1f,   2f,  0.5f,  0.5f, 2f,   1f,   1f},
		/*GRASS*/       new float[] {1f,  0.5f,  2f,   2f,   0.5f, 1f,   1f,  0.5f},
		/*POIS*/        new float[] {1f,   1f,   1f,   1f,   2f,   1f,   1f,   1f},
    };

    public static float GetEffectiveness(MonsterType attackType, MonsterType defenseType)
    {
        if (attackType == MonsterType.None || defenseType == MonsterType.None)
            return 1;
        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}