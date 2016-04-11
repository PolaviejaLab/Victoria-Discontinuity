using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class ICExperimentBrowserController: MonoBehaviour 
{
    public ICExperimentSetupController experimentSetup;

    public ICListBox listBox;
    public Button configureButton;
    public Button joinButton;

    private ICExperiment[] experiments;


    /**
     * Enumerate ICExperiments available in the scene.
     */
    void Awake()
    {
        if(!experimentSetup) throw new Exception("experimentSetup field not set.");

        experiments = GameObject.FindObjectsOfType<ICExperiment>();
    }



    /**
     * Add items from experiments field to listbox.
     */
    private void RefreshExperimentListBox()
    {
        if(listBox == null) return;

        listBox.items.Clear();
        for(var i = 0; i < experiments.Length; i++) {
            if(experiments[i] == null) continue;

            string name = experiments[i].getDisplayName();
            listBox.items.Add(i.ToString(), name);
        }
    }


    /**
     * Open experiment configuration interface.
     */
    private void OpenConfigureInterface()
    {        
        if(listBox == null) return;

        int selection;

        if(!int.TryParse(listBox.selectedItem, out selection)) {
            Debug.LogWarning("Cannot configure experiment, invalid selection.");
            return;
        }

        var experiment = experiments[selection];

        if(experiment.maximumParticipants > 1) {
            experimentSetup.StartServer(experiment);
        } else {
            SceneManager.LoadScene(experiment.sceneName);
        }
    }


    /**
     * Start search for servers.
     */
    private void OpenJoinInterface()
    {
        experimentSetup.StartServerBrowser();
    }


	void Start () 
    {
        RefreshExperimentListBox();

        if(configureButton) {
            configureButton.onClick.RemoveAllListeners();
            configureButton.onClick.AddListener(OpenConfigureInterface);
        }

        if(joinButton) { 
            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(OpenJoinInterface);
        }
	}   
}
