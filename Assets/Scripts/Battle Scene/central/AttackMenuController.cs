//using UnityEngine;
//using UnityEngine.UI;

//public class AttackMenuController : MonoBehaviour
//{
//    public GameObject buttonPrefab;
//    public Transform buttonContainer;

//    private CharacterStats currentPlayer;

//    public void ShowAttacks(CharacterStats player)
//    {
//        currentPlayer = player;
//        foreach (Transform t in buttonContainer) Destroy(t.gameObject);
//        foreach (var attack in player.attacks)
//        {
//            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
//            Button btn = btnObj.GetComponent<Button>();
//            btn.GetComponentInChildren<Text>().text = attack.attackName;
//            Attack capturedAttack = attack;
//            btn.onClick.AddListener(() => OnAttackButtonClicked(capturedAttack));
//        }
//    }

//    void OnAttackButtonClicked(Attack attack)
//    {
//        CharacterStats target = TurnManager.Instance.enemyParty[0];

//        CombatSystem.Instance.ExecuteAttack(currentPlayer, target, attack);

//        TurnManager.Instance.EndTurn();

//        gameObject.SetActive(false);
//    }
//}
