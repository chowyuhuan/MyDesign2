using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Network;

namespace UI {

    public enum MailStatus
    {
        UnRead,
        Read,
    }

    public class UIMailItem : ExtrScrollItem, IPointerClickHandler
    {
        [Header("图标")]
        [SerializeField]
        Image icon;
        [Header("时间")]
        [SerializeField]
        Text time;
        [Header("标题")]
        [SerializeField]
        Text title;
        [Header("状态")]
        [SerializeField]
        Text status;

        public MailConfig mailConfig;

        public Image attachmentFlag;

        public Image statusFlag;

        private immortaldb.MailItem mailItem;

        void OnEnable()
        {
            NetworkManager.RegisterHandler((uint)gateproto.command.CMD_READ_MAIL_RSP, OnReadMail);
            NetworkManager.RegisterHandler((uint)gateproto.command.CMD_DRAW_MAIL_RSP, OnDrawMail);
        }
        
        void OnDisable()
        {
            NetworkManager.UnregisterHandler((uint)gateproto.command.CMD_READ_MAIL_RSP, OnReadMail);
            NetworkManager.UnregisterHandler((uint)gateproto.command.CMD_DRAW_MAIL_RSP, OnDrawMail);
        }

        private string ConvertTimeFormat(long timeStamp)
        {
            DateTime dateTime = new DateTime(timeStamp);

            return dateTime.ToString();
        }

        public void InitMailItem(immortaldb.MailItem item) 
        {
            mailItem = item;
            time.text = ConvertTimeFormat(item.time_stamp);
            icon.sprite = mailConfig.GetIcon((int)item.mail_type);
            title.text = mailConfig.GetTitle((int)item.mail_type);
            attachmentFlag.gameObject.SetActive(DataCenter.HaveItemInMail(item));
            SetRead(item.is_read);
        }

        public void SetRead(bool read)
        {
            status.text = read ? MailStatus.Read.ToString() : MailStatus.UnRead.ToString();
            statusFlag.gameObject.SetActive(read);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            MCEvent detail = new MCEvent(MCEventType.MAIL_DETAIL);
            detail.ObjectValue = mailItem;
            MCEventCenter.instance.dispatchMCEvent(detail);

            DataCenter.ReadOneMailRequest(mailItem);
        }
        
        private void OnReadMail(ushort result, object response, object request)
        {
            if (result == 0)
            {
                gateproto.ReadMailRsp rsp = response as gateproto.ReadMailRsp;
                
                if (rsp.mail_id == mailItem.mail_id)
                {
                    SetRead(true);
                }
            }
        }

        private void OnDrawMail(ushort result, object response, object request)
        {
            if (result == 0)
            {
                gateproto.DrawMailRsp rsp = response as gateproto.DrawMailRsp;
                
                if (rsp.mail_id == mailItem.mail_id)
                {
                    SetRead(true);
                }
            }
        }
    }
}

