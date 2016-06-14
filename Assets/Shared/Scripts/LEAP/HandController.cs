using UnityEngine;
using System.Collections;
using System.IO;
using System;

using Leap;
using Leap.Unity;

/**
 * Our own extensions to the LEAP software.
 */
public class HandController : MonoBehaviour 
{
    public LeapHandController handController;
    public IHandModel[] models;

    private StreamWriter streamWriter;

    // Use this for initialization
    void Start ()
    {	
	}
	

	// Update is called once per frame
	void Update ()
    {
        if (!Application.isPlaying) return;
        if (!handController) return;

        UpdateRecorder();

        Frame frame = handController.Provider.CurrentFrame;

        foreach(var hand in frame.Hands) {
            foreach(var model in models) {
                if (model == null) continue;
                if (!model.enabled) continue;

                if (model.Handedness == Chirality.Left && !hand.IsLeft) continue;
                if (model.Handedness == Chirality.Right && !hand.IsRight) continue;

                model.SetLeapHand(hand);
                model.UpdateHand();
            }
        }
    }


    public void StartRecording(string filename) 
    {
        streamWriter = new StreamWriter(filename, true);
    }

    public void StopRecording() 
    {
        streamWriter.Close();
        streamWriter = null;
    }

    /**
	 * Writes a 3d vector to file
	 */
    private void WritePosition(StreamWriter writer, Vector vector) {
        writer.Write(vector.x);
        writer.Write(", ");
        writer.Write(vector.y);
        writer.Write(", ");
        writer.Write(vector.z);
    }


    protected void UpdateRecorder() 
    {
        if (streamWriter == null || handController == null)
            return;

        Frame frame = handController.Provider.CurrentFrame;
        bool gotHand = false;

        streamWriter.Write(DateTime.UtcNow.ToString("o") + ", ");

        int num_hands = frame.Hands.Count;
        for (int h = 0; h < num_hands; ++h) {
            if (frame.Hands[h].IsRight) {
                gotHand = true;

                streamWriter.Write(frame.Hands[h].Confidence);
                streamWriter.Write(", ");

                WritePosition(streamWriter, frame.Hands[h].PalmPosition);
                break;
            }
        }

        if (!gotHand) {
            streamWriter.Write("False, 0, 0, 0");
        }

        streamWriter.WriteLine();
        streamWriter.Flush();
    }
}
