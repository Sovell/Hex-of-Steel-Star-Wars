public class BattleContext
{
	public Unit AttackerUnit { get; private set; }

	public Unit TargetUnit { get; private set; }

	public DamageTypes DamageType { get; private set; }

	public bool IsRetaliation { get; private set; }

	public bool IsSupportiveFire { get; private set; }

	public BattleContext(Unit p_attackerUnit, Unit p_targetUnit, DamageTypes p_damageType, bool p_isRetaliation, bool p_isSupportiveFire)
	{
		AttackerUnit = p_attackerUnit;
		TargetUnit = p_targetUnit;
		DamageType = p_damageType;
		IsRetaliation = p_isRetaliation;
		IsSupportiveFire = p_isSupportiveFire;
	}
}
