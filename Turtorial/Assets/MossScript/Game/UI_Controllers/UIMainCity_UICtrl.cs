using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;



public class UIMainCity_UICtrl : UI_ctrl {

	public override void Awake() {
		base.Awake();

	}

	void Start() {
		this.view["Container_LeftTop/RoleInfo/RoleInfoBG/lblNickName"].GetComponent<Text>().text = "BYCW";
		this.add_button_listener("Container_RightBottom/UIMainCitySkillView/BtnSkill1", this.onStartClick);
	}

	private void onStartClick() {
		Debug.Log("onStartClick!!!!");
	}
}
