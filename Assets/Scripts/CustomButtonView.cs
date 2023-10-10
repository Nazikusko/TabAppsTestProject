using System.Collections;
using DG.Tweening;
using System.Drawing;
using TMPro;
using UnityEngine;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;

public class CustomButtonView : MonoBehaviour
{
    [SerializeField] private Image _buttonImage;
    [SerializeField] private TMP_Text _buttonText;
    [SerializeField] private ParticleSystem _particlesVfxPrefab;

    private ButtonsDataModel _buttonDataModel;

    public ButtonsDataModel ButtonDataModel => _buttonDataModel;
    public void SetView(string buttonText, Color buttonColor, ButtonsDataModel buttonDataModel)
    {
        _buttonDataModel = buttonDataModel;
        _buttonImage.color = buttonColor;
        _buttonText.text = buttonDataModel.id + " " + buttonText;

        _buttonImage.transform.DOKill();
        var sequence = DOTween.Sequence()
            .Append(_buttonImage.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutCubic))
            .Append(_buttonImage.transform.DOScale(1f, 0.4f).SetEase(Ease.InCubic));
        sequence.SetLoops(3, LoopType.Restart);

        StartCoroutine(StartParticles());
    }

    private IEnumerator StartParticles()
    {
        yield return null;

        var point = Camera.main.ScreenToWorldPoint(new Vector3(transform.position.x, transform.position.y, 10f));
        point = new Vector3(point.x - 0.9f, point.y, point.z);
        var particle = Instantiate(_particlesVfxPrefab, point, Quaternion.identity);
        Color.RGBToHSV(_buttonImage.color, out var h, out var s, out var v);
        particle.GetComponent<ParticleSetup>().SetupParticle(Color.HSVToRGB(h, 1f, v), 0.35f);
        particle.Play(true);
        Destroy(particle.gameObject, 4f);
    }
}
