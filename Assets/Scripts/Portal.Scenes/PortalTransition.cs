using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class PortalTransition : MonoBehaviour
{
    [Header("Destino")]
    public string targetScene;       // ex: "SCN_Fase2"
    public string targetEntryId;     // ex: "fromFase1"

    [Header("UI")]
    public Canvas promptCanvas;      // um Canvas world-space simples com texto "Segure B para viajar"
    public Image holdFillImage;      // opcional: imagem radial para mostrar progresso

    [Header("Input")]
    public KeyCode holdKey = KeyCode.B;
    public float holdTime = 1.5f;

    private bool _playerInside;
    private float _holdTimer;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInside = true;
            _holdTimer = 0f;
            if (promptCanvas) promptCanvas.enabled = true;
            if (holdFillImage) holdFillImage.fillAmount = 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInside = false;
            _holdTimer = 0f;
            if (promptCanvas) promptCanvas.enabled = false;
            if (holdFillImage) holdFillImage.fillAmount = 0f;
        }
    }

    private void Update()
    {
        if (!_playerInside) return;
        if (Input.GetKey(holdKey))
        {
            _holdTimer += Time.deltaTime;
            if (holdFillImage) holdFillImage.fillAmount = Mathf.Clamp01(_holdTimer / holdTime);

            if (_holdTimer >= holdTime)
            {
                // viajar!
                if (!string.IsNullOrEmpty(targetScene))
                {
                    GameManager.Instance.TrocarCena(targetScene, targetEntryId);
                }
            }
        }
        else
        {
            if (_holdTimer > 0f)
            {
                _holdTimer = 0f;
                if (holdFillImage) holdFillImage.fillAmount = 0f;
            }
        }
    }
}
