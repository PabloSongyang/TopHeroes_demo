using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamage
{
    int Damage { get; }

    IEntity SelfEntity { get; }
    void TakeDamage(IEntity target, int damage, Vector2 hitPosition, Vector2 hitDirection, float beatBackDis, SoundInfo hitSoundInfo, string hitEffectLabel = null);
}
