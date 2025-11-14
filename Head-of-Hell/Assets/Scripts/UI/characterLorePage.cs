using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class CharacterLore : MonoBehaviour
{
    public TextMeshProUGUI infoTitle; // Reference to the info title text
    public TextMeshProUGUI loreTitle; // Reference to the lore title text
    public TextMeshProUGUI loreInfo; // Reference to display the lore content
    public GameObject nextButton; // Reference to the next button GameObject
    public GameObject previousButton; // Reference to the previous button GameObject

    private string[] loreLines; // Array to hold each line of the lore text
    private int currentPage = 0; // Current page index
    private const int maxLinesPerPage = 10; // Max number of lines per page

    // Method to set the lore and log it
    public void SetLore(string lore)
    {
        Debug.Log("Ok");
        if (!string.IsNullOrEmpty(lore))
        {
            Debug.Log("Lore: " + lore); // Log the lore text to the console
            loreTitle.text = infoTitle.text;

            // Split the lore into lines
            loreLines = SplitLoreIntoLines(lore);

            // Display the first page
            currentPage = 0;
            DisplayPage(currentPage);

            // Check button visibility
            UpdateButtonVisibility();
        }
        else
        {
            Debug.LogError("Error: Lore text is empty or null!");
        }
    }

    private string[] SplitLoreIntoLines(string lore)
    {
        Debug.Log("Lines");
        List<string> lines = new List<string>();
        string[] words = lore.Split(' ');

        string currentLine = "";
        foreach (var word in words)
        {
            if (currentLine.Length + word.Length + 1 > 40) // Check if adding the next word exceeds 40 characters
            {
                lines.Add(currentLine);
                currentLine = word; // Start a new line with the current word
            }
            else
            {
                currentLine += (currentLine.Length > 0 ? " " : "") + word; // Add the word to the current line
            }
        }
        if (currentLine.Length > 0) // Add the last line if it has content
        {
            lines.Add(currentLine);
        }

        return lines.ToArray();
    }

    private void DisplayPage(int pageIndex)
    {
        Debug.Log("DPage");
        int startLine = pageIndex * maxLinesPerPage;
        int endLine = Mathf.Min(startLine + maxLinesPerPage, loreLines.Length);

        
        string loreToDisplay = "";
        for (int i = startLine; i < endLine; i++)
        {
            loreToDisplay += loreLines[i] + "\n"; // Add each line to the display text
            Debug.Log(loreToDisplay);
        }

        loreInfo.text = loreToDisplay; // Set the displayed text
    }

    private void UpdateButtonVisibility()
    {
        nextButton.SetActive(currentPage < (loreLines.Length / maxLinesPerPage));
        previousButton.SetActive(currentPage > 0);
    }

    public void NextPage()
    {
        if (currentPage < (loreLines.Length / maxLinesPerPage))
        {
            currentPage++;
            DisplayPage(currentPage);
            UpdateButtonVisibility();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            DisplayPage(currentPage);
            UpdateButtonVisibility();
        }
    }
}
