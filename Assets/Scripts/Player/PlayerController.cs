﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;


//------------------------------------------------------------------------
//
//  Name:   PlayerController.cs
//
//  Desc:   Define the player controlls from walking, standing, jumping and others
//
//  Attachment : This class is used in the "Player" GameObject
//
//  Last modification :
//      AA - 02/11/2017 : Init version
//      RM - 03/11/2017 : Reorganisation of Update (with HandleInputs) and FixedUpdate (new methods)
//------------------------------------------------------------------------




public class PlayerController : MonoBehaviour
{
    
    public ShotScript shotScript;

    // Components
    //-----------
    private Rigidbody2D rigidBody2D;
    private Animator animator;
    private AudioSource audioSource;

    // Others
    //-------
    public float speedForce;
    public float jumpForce;
    public AudioClip soundJump, soundDead, hurt;
    private float horizontal;
    public bool isGrounded;
    private bool isFacingRight;
    //private bool attack;
    private bool dead;
    private bool jump;
    [SerializeField] private bool keyboardOrAndroidInputs;

    //Système santé
    public int maxHealth = 100;
    private int currentHealth;
    public Slider healthSlider;

    //Système dégats
    //private GameObject dinoGround;
    //private GroundEnemiesAI dinoGroundHealth;
    //private bool dinoInRange;
    //public int damageAttack = 1;
    //public float timeAttackCooldown = 0.2f;
    //private float currentTimeAttackCooldown;

    //Game over and score system
    public GameOverScript gameOver;

    //---------------------------------------------------------
    // Method called when the game starts
    //---------------------------------------------------------
    void Start() {

        isGrounded = false;
        isFacingRight = true;
        //attack = false;
        dead = false;
        jump = false;

        //Santé
        currentHealth = maxHealth;

        //Dégats
        //dinoGround = GameObject.FindGameObjectWithTag("GroundDino");
        //dinoGroundHealth = dinoGround.GetComponent<GroundEnemiesAI>();

        //Get the RigidBody 2D of the player
        rigidBody2D = GetComponent<Rigidbody2D>();
        // Get the Animator of the player
        animator = GetComponent<Animator>();
        // Get the AudioSource of the player
        audioSource = GetComponent<AudioSource>();
    }


    //---------------------------------------------------------
    // Method called every frame
    //---------------------------------------------------------
    void Update() {
        
        if (!dead) {

            handleInputs();

            //Flip player if needed
            playerFlip();

            // Translate the player 
            // Vector2.right is (1,0) : used for direction
            // h the horizatal axe input : used to dertermine the movement right or left and the speed of player
            // speed the variable of speed, it is fixed
            // Time.deltaTime it is used to standardize the translation speed according to the machines' power 
            transform.Translate(Vector2.right * horizontal * speedForce * Time.deltaTime);
        }

    }

    //---------------------------------------------------------
    // It is not advised in Unity to use update in Rigidbody manipulations
    // Called every physics steps (regular time interval between frames)
    //---------------------------------------------------------
    void FixedUpdate() {

        if (!dead) {

            //Run movement
            if (horizontal == 0) playerStopRun();
            else playerRun();

            if (isGrounded && jump) playerJump();
            //if (attack) playerAttack();

            //If player falls through ground
            if (transform.position.y < -65) playerDead(true);

            resetValues();

        }

    }

    //---------------------------------------------------------
    //Reset values for updates
    //---------------------------------------------------------
    private void resetValues() {
        jump = false;
        //attack = false;
    }

    //---------------------------------------------------------
    //This method is called in Update() to handle user's inputs
    //---------------------------------------------------------
    private void handleInputs() {

        // GetAxis("Horizontal") :
        // When no direction button is clicked the return is 0.0
        // If the left is clicked the return is in range of ]0.0,-1.0]
        // If the right is clicked the return is in range of ]0.0,1.0]

        // Check "Keyboard Inputs" in the player's inspector to
        //activate keyboard contols (and disable android controls)

        if (keyboardOrAndroidInputs) {
            /*-------------------*/
            /* Keyboard controls */
            /*-------------------*/
            horizontal = Input.GetAxis("Horizontal");
            //if (Input.GetKeyDown(KeyCode.H)) attack = true;
            if (Input.GetKeyDown(KeyCode.Space)) jump = true;
        }
        else {
            /*------------------*/
            /* Android controls */
            /*------------------*/
            // When no direction button is clicked the return is 0.0
            // If the left is clicked the return is in range of ]0.0,-1.0]
            // If the right is clicked the return is in range of ]0.0,1.0]
            horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            if (CrossPlatformInputManager.GetButtonDown("Jump")) jump = true;
            //if (CrossPlatformInputManager.GetButtonDown("Attack")) attack = true;
        }

    }

    //---------------------------------------------------------
    //Player's flipping control
    //---------------------------------------------------------
    private void playerFlip() {
        if (horizontal > 0 && !isFacingRight || horizontal < 0 && isFacingRight) {
            isFacingRight = !isFacingRight;
            Vector2 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;

            //Shoot other way if direction change
            Vector2 temp = shotScript.direction;
            shotScript.direction = temp * -1;
        }
    }

    //---------------------------------------------------------
    // This method is called when the player runs
    // /!\ This mehtod is not complet /!\
    //---------------------------------------------------------
    public void playerRun() {
        animator.SetBool("run", true);
    }

    //---------------------------------------------------------
    // This method is called when the player is not moving
    // /!\ This mehtod is not complet /!\
    //---------------------------------------------------------
    public void playerStopRun() {
        animator.SetBool("run", false);
    }

    //---------------------------------------------------------
    // This method is called when the player jumps
    // /!\ This mehtod is not complet /!\
    //---------------------------------------------------------
    public void playerJump() {
        animator.SetTrigger("jump");
        //rigidBody2D.velocity = new Vector2(rigidBody2D.velocity.x, rigidBody2D.velocity.y + jumpForce);
        rigidBody2D.AddForce(new Vector2(0, jumpForce));
        audioSource.PlayOneShot(soundJump);
    }


    //---------------------------------------------------------
    // This method is called when the player is hurt
    //---------------------------------------------------------
    public void playerHurt(int amount, bool dinoFacingRight) {
        if (!dead) {
            currentHealth -= amount;
            if (currentHealth < 0) currentHealth = 0;
            healthSlider.value = currentHealth;

            if (currentHealth == 0) {

                playerDead(dinoFacingRight);
            }
            else {

                if (isFacingRight == dinoFacingRight) animator.SetTrigger("hurt_back");
                else animator.SetTrigger("hurt_front");

                if (!audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(hurt);
                }
            }
        }
    }


    //---------------------------------------------------------
    // This method is called when the player is dead
    // /!\ This mehtod is not complet /!\
    //---------------------------------------------------------
    public void playerDead(bool dinoFacingRight) {
        dead = true;

        // Play death animation
        if (isFacingRight == dinoFacingRight) animator.SetTrigger("dead_back");
        else animator.SetTrigger("dead_front");
        
        audioSource.PlayOneShot(soundDead);   // play death sound

        //Pause le jeu
        Time.timeScale = 0;
        gameOver.ShowPanel();
    }

 
    //---------------------------------------------------------
    // This method is called when the player attacks (hit)
    // /!\ This mehtod is not complet /!\
    //---------------------------------------------------------
   /* public void playerAttack() {
        animator.SetTrigger("hit");
        audioSource.PlayOneShot(soundHit);   // play hit sound

        
    }*/


    //---------------------------------------------------------
    // This method is called when the player catch a ingredient
    //---------------------------------------------------------
    /*public void OnTriggerEnter2D(Collider2D other) {

        if (other.gameObject.CompareTag("Ingredient")) {
            other.gameObject.SetActive(false);
            countFruit = countFruit + 1;
            SetTextFruit();
        }
    }*/

    /*void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject == dinoGround) {
            dinoInRange = false;
            playerRun();
            print("exit collider");
        }
    }*/


    public int getCurrentHealth() {
        return currentHealth;
    }

 
}