using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBehavior : MonoBehaviour {

    private Animator playerAnimator;

	// Use this for initialization
	void Start () {
        playerAnimator = this.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        checkInput();
	}

    public void checkInput() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            attack();
        }
        else if (Input.GetKeyDown(KeyCode.W)) {
            attackSpell();
        }
        else if (Input.GetKeyDown(KeyCode.E)) {
            takeDamage();
        }
        else if (Input.GetKeyDown(KeyCode.R)) {
            death();
        }
        else if (Input.GetKeyDown(KeyCode.T)) {
            walk();
        }
    }

    public void attack() {
        playerAnimator.Play("Shadow_Attack");
    }

    public void attackSpell() {
        playerAnimator.Play("Shadow_Spell");
    }

    public void takeDamage() {
        playerAnimator.Play("Shadow_Damaged");
    }

    public void death() {
        playerAnimator.Play("Shadow_Killed");
    }

    public void walk() {
        playerAnimator.Play("Shadow_Walk");
    }

}
