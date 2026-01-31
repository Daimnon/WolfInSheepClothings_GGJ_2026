using TMPro;
using UnityEngine;

public class TMP_Wave : MonoBehaviour
{
    [SerializeField] private float amplitude = 10f;
    [SerializeField] private float frequency = 2f;
    [SerializeField] private float speed = 2f;

    private TMP_Text _text;
    private TMP_TextInfo _textInfo;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        _text.ForceMeshUpdate();
        _textInfo = _text.textInfo;

        float time = Time.time * speed;

        for (int i = 0; i < _textInfo.characterCount; i++)
        {
            var charInfo = _textInfo.characterInfo[i];
            if (!charInfo.isVisible)
                continue;

            int meshIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] vertices = _textInfo.meshInfo[meshIndex].vertices;

            float wave = Mathf.Sin(time + i * frequency) * amplitude;
            Vector3 offset = Vector3.up * wave;

            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        for (int i = 0; i < _textInfo.meshInfo.Length; i++)
        {
            _textInfo.meshInfo[i].mesh.vertices = _textInfo.meshInfo[i].vertices;
            _text.UpdateGeometry(_textInfo.meshInfo[i].mesh, i);
        }
    }
}