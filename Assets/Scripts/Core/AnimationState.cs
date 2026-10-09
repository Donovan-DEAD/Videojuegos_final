namespace MadnessDreams
{
    /// <summary>
    /// Estados de animación compartidos que los enemigos escriben en su <c>Animator</c>.
    /// </summary>
    public enum AnimationState
    {
        Idle = 0,
        Move = 1,
        MeleeAttack = 2,
        RangedAttack = 3,
        Death = 4
    }
}
