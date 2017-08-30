using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Network;

namespace UI 
{
    public class UIMailDetail : GUI_Window
    {
        public Text content;

        public Image[] items;

        public Text[] itemNumbers;

        public Button getButton;

        public Button gotButton;

        private immortaldb.MailItem mailItem;

        void OnEnable()
        {
            NetworkManager.RegisterHandler((uint)gateproto.command.CMD_DRAW_MAIL_RSP, OnDrawMail);
        }
        
        void OnDisable()
        {
            NetworkManager.UnregisterHandler((uint)gateproto.command.CMD_DRAW_MAIL_RSP, OnDrawMail);
        }

        private void ResetItemList()
        {
            for (int i = 0; i < items.Length; i++)
            {
                SetItemEnable(i, false);
            }
        }

        private void SetItemEnable(int i, bool flag)
        {
            items[i].gameObject.SetActive(flag);
            itemNumbers[i].gameObject.SetActive(flag);
        }

        private void SetGetButtonState(bool flag, bool exist)
        {
            if (flag)
            {
                getButton.gameObject.SetActive(exist);
                gotButton.gameObject.SetActive(!exist);
            }
            else
            {
                getButton.gameObject.SetActive(false);
                gotButton.gameObject.SetActive(false);
            }
        }

        public void InitMailDetail(immortaldb.MailItem item)
        {
            mailItem = item;

            content.text = mailItem.content;

            ResetItemList();

            for (int i = 0; i < mailItem.item_list.Count; i++)
            {
                items[i].sprite = GetItemIcon(mailItem.item_list[i]);
                itemNumbers[i].text = mailItem.item_list[i].item_count.ToString();
                SetItemEnable(i, true);
            }

            SetGetButtonState(DataCenter.HaveItemInMail(mailItem), DataCenter.ExistMail(mailItem.mail_id));
        }

        private Sprite GetItemIcon(immortaldb.MailItemInfo itemInfo)
        {
            CSV_b_item_template data =  CSV_b_item_template.FindData((int)itemInfo.item_id);
            return data == null ? null : SpriteConfig.FindSprite(data.Icon);
        }

        public void OnGetButtonClick()
        {
            DataCenter.DrawOneMailRequest(mailItem);
        }

        private void OnDrawMail(ushort result, object response, object request)
        {
            if (result == 0)
            {
                gateproto.DrawMailRsp rsp = response as gateproto.DrawMailRsp;
                
                if (rsp.mail_id == mailItem.mail_id)
                {
                    SetGetButtonState(DataCenter.HaveItemInMail(mailItem), DataCenter.ExistMail(mailItem.mail_id));
                }
            }
        }
    }
}
