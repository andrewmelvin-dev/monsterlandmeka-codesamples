using System.Collections.Generic;
using UnityEngine;

public class MM_TalkDialog {
    public PersistentObject persistentObject;
    public string title;
    public string portraitSprite;
    public Automator onCompleteAutomator;
    public DialogPurchase dialogPurchase;
    public List<string> textBatch;
    public UILabelTalk? nextLabelTalk;
    public int? selection;
    public MM_TalkDialog(string text) {
        // The text passed in should be in the following structure:
        // id|title|portraitSprite|onCompleteAutomator|textBatch1|textBatch2
        this.persistentObject = PersistentObject.NOTHING;
        this.title = "";
        this.portraitSprite = "";
        this.onCompleteAutomator = Automator.NONE;
        this.dialogPurchase = DialogPurchase.NONE;
        this.textBatch = new List<string>();
        this.nextLabelTalk = null;
        this.selection = null;
        if (text.Length > 0) {
            string[] data = text.Split('|');
            if (data.Length > 0) {
                if (data[0] != "" && int.TryParse(data[0], out int id)) {
                    this.persistentObject = (PersistentObject)id;
				}
                this.title = data[1];
            }
            if (data.Length > 1) {
                this.portraitSprite = data[2];
            }
            // The onCompleteAutomator is triggered when the talk dialog is closed
            if (data.Length > 2 && data[3] != "" && int.TryParse(data[3], out int automator)) {
                this.onCompleteAutomator = (Automator)automator;
            }
            // The dialogPurchase is used to cause purchase options to appear after the last textbatch has been processed
            if (data.Length > 3 && data[4] != "" && int.TryParse(data[4], out int purchase)) {
                this.dialogPurchase = (DialogPurchase)purchase;
            }
            // Everything else in the string payload will be the batches of text that should appear
            if (data.Length > 5) {
                for (int i = 5; i < data.Length; i++) {
                    // If the current text batch is numeric then it represents the next UILabelTalk to move to
                    if (data[i] != "" && int.TryParse(data[i], out int uiLabelTalk)) {
                        this.nextLabelTalk = (UILabelTalk)uiLabelTalk;
                    } else {
                        this.textBatch.Add(data[i]);
                    }
                }
            }
        }
    }
}
