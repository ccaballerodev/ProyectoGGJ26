using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "TextSequence",
    menuName = "Narrative/Text Sequence"
)]
public class TextSequenceSO : ScriptableObject
{
    [TextArea(3, 6)]
    public List<string> lines;
}