using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Network;

namespace UI 
{
    public class UIMailPage : GUI_Window, IMCEventListener
    {
        public Transform scrollMailList;

        public UIMailDetail uiMailDetail;

        public UIMailItem uiMailItem;

        public ToggleGroup toggleGroup;

        public Button getAllButton;
        
        public Button gotAllButton;

        public override void PreShowWindow()
        {
            MCEventCenter.instance.dispatchMCEvent(new MCEvent(MCEventType.GUIDE_NOT_MANDATORY_FINISH));
        }

        void OnEnable()
        {
            MCEventCenter.instance.registerMCEventListener(MCEventType.MAIL_DETAIL, this);

            InitMailList();
        }
        
        void OnDisable()
        {
            MCEventCenter.instance.unregisterMCEventListener(MCEventType.MAIL_DETAIL, this);
        }

        private void InitMailList()
        {
            //DataCenter.MailListSampler();
            DataCenter.SortMailList();

            foreach (immortaldb.MailItem mailItem in DataCenter.sortedMailList)
            {
                UIMailItem item = GameObject.Instantiate<UIMailItem>(uiMailItem);
                item.InitMailItem(mailItem);
                item.transform.SetParent(scrollMailList);
                item.GetComponent<Toggle>().group = toggleGroup;
            }

            SetGetAllButtonState(DataCenter.mailList.Count > 0);
        }

        public void OnGetAllButtonClick()
        {
            DataCenter.DrawAllMailRequest();

            SetGetAllButtonState(false);
        }

        private void SetGetAllButtonState(bool enable)
        {
            getAllButton.gameObject.SetActive(enable);
            gotAllButton.gameObject.SetActive(!enable);
        }

        public void OnEvent(MCEvent evt)
        {
            switch (evt.Type)
            {
                case MCEventType.MAIL_DETAIL:
                    PlayUIMailDetailAppear((immortaldb.MailItem)evt.ObjectValue);
                    break;
            }
        }

        private void PlayUIMailDetailAppear(immortaldb.MailItem mailItem)
        {
            uiMailDetail.InitMailDetail(mailItem);
            uiMailDetail.gameObject.SetActive(true);
            GUI_TweenerUtility.ResetAndPlayTweeners(uiMailDetail.gameObject);
        }
    }
}
