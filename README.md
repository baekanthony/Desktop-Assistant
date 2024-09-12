# Desktop Assistant

![Assistant](https://github.com/user-attachments/assets/ff782714-5de1-4234-b016-a9b9fbc93edd)

## About
Microsoft Agent desktop assistant designed to enhance productivity and improve quality of life through useful commands.<br> 

The assistant understands natural language and can recognize intents, allowing for natural and flexible phrases without having to memorize commands.<br>
(e.g., 'Open Chrome,' 'Launch Chrome,' or 'Boot up Chrome' all recognized as the same command)<br>
<br>
Inspired by Bonzi Buddy and Clippy, users can also upload their own agent characters for better interaction and entertainment.

## Technologies
* `C#, .NET, and WPF` for building the Windows desktop application
* `Azure CLU` for natural language processing to classify user intent and execute commands

## Supported features

#### Launching programs  
Launches any applications listed in a designated programs folder (can be set in settings)<br>
`e.g. "Open firefox"`

#### Swapping monitor contents
Swaps the contents of two monitors. Window sizes are scaled if necessary, and relative positions are preserved. Identified monitors can be found in settings, and numbered as pleased. (Useful for swapping between documentation and code)<br>
`e.g. "Swap monitors 1 and 3"`

#### Changing monitor brightness <mark>**(Work in progress)**</mark>
Allows users to adjust a monitor's brightness without having to press the physical buttons on the monitor.<br>

## Planned Features
* Speech to text for speaking commands
* Focus mode
* Add more interaction with agent
* Improve responsiveness (clearer feedback on commands and errors)
* Quick notes

## Limitations
As Azure is a paid service, it's not very sustainable (Especially since my free trial is running out). It would also be a notable limitation if I were to imagine this as a product other people use. 
Having the assistant be able to function offline would also be nicer. I plan on exploring different options with hardcoded commands as a last resort. 
