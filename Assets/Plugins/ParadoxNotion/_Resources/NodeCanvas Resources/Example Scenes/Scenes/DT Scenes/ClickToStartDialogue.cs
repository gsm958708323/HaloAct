using UnityEngine;
using System.Collections;
using NodeCanvas.DialogueTrees;

public class ClickToStartDialogue : MonoBehaviour
{

    public DialogueTreeController dialogueController;

    void OnMouseDown() {
        gameObject.SetActive(false);
        dialogueController.StartDialogue(OnDialogueEnd);
    }

    void OnDialogueEnd(bool success) {
        gameObject.SetActive(true);
    }
}