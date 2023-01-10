using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI _text;
    public string[] _lines;
    public float _textSpeed;
    private int _index;

    private void Start() {
        _text.text = string.Empty;
        StartDialogue();
    }
    private void Update() {
        if(Input.GetKeyDown(KeyCode.E))
        {
            if(_text.text == _lines[_index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                _text.text = _lines[_index];
            }
        }
    }
    private void StartDialogue()
    {
        _index = 0;
        StartCoroutine(TypeLine());
    }

    private void NextLine()
    {
        if(_index < _lines.Length -1)
        {
            _index++;
            _text.text = string.Empty;
             StartCoroutine(TypeLine());
        }

        else
        {
            gameObject.SetActive(false);
        }
    }

    IEnumerator TypeLine()
    {
        foreach (char l in _lines[_index].ToCharArray())
        {
            _text.text += l;
            yield return new WaitForSeconds(_textSpeed);
        }
    }
}