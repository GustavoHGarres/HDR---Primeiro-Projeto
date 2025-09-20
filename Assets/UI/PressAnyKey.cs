using UnityEngine;

public class PressAnyKey : MonoBehaviour
{
    [SerializeField] private KeyCode debugKey = KeyCode.Return; // Enter funciona no PC

    private bool _armed = false;

    private void OnEnable()
    {
        _armed = false;
        Invoke(nameof(Arm), .2f); // evita pegar teclas da cena anterior
    }

    void Arm() => _armed = true;

    void Update()
    {
        if (!_armed) return;

        if (Input.anyKeyDown || Input.GetKeyDown(debugKey))
        {
            GameManager.Instance.GoToMenu();
            enabled = false;
        }
    }
}
