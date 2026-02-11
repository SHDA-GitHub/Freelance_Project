using System.Collections;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    public static CombatSystem Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ExecuteAttack(CharacterStats attacker, CharacterStats target, Attack attack)
    {
        if (attacker.currentPP < attack.powerCost) return;

        attacker.currentPP -= attack.powerCost;
        target.ReceiveDamage(attack.damage);

        if (attack.attackSound != null)
            AudioSource.PlayClipAtPoint(attack.attackSound, target.transform.position);
    }

    public IEnumerator FlashDamageEffect(CharacterStats target)
    {
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        for (int i = 0; i < 3; i++)
        {
            sr.color = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(0.1f);
            sr.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(0.1f);
        }
    }
}