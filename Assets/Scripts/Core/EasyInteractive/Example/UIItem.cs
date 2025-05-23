using Core.EasyInteractive;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace Core.EasyInteractive.Example
{
	/// <summary>
	/// UI对象
	/// </summary>
	public class UIItem : InteractableUIElement, IDragable
	{
		public Image icon;
		private bool _enableDrag = true;

		public Type interactTag => typeof(UIItem);
		public bool enableFocus => true;
		public bool enableDrag => _enableDrag;
		private void Update()
		{
			_enableDrag = icon.gameObject.activeSelf;
		}
		public void OnFocus()
		{
		}
		public void EndFocus()
		{
		}
		public void OnDrag()
		{
			if (!icon.gameObject.activeSelf) return;
			GhostIcon.Instance.ShowGhostIcon(icon.sprite);
			icon.gameObject.SetActive(false);
		}
		public void ProcessDrag()
		{
		}
		public void EndDrag(IFocusable target)
		{
			GhostIcon.Instance.HideGhostIcon();
			//判断是否拖拽到了目标
			if (target == null)
				icon.gameObject.SetActive(true);
		}
	}
}
