Configurable Heads-Up Display
========

What it does

How to use:
-----------

	Setup:
    
    Components of the HUD are located in Assets/HUD
    DisplayFeeds must be in scene to be used
    Under modules there must be a HudManager prefab with the HudManager script attached 
    
    In game:
    
    1. Open the controller menu
    2. Change scenes to a scene other than the lobby
    3. Open the controller menu navigate to edit/interface
    4. Under interface there is a header for "User Interface"

        World Space Displays
        a. Select the display icon
        b. hit the plus icon to add a display to the scene, if closed this menu can be used to reopen them

        Heads-Up Displays
        a. Select the HUD icon (eyeball for now)
        b. The first tab is for loading and saving
        c. the second tab is for editing the HUD, here you can add displays, push them to the HUD, and toggle the HUD
        d. the third tab is for settings for the HUD, you can change the threshold the HUD follows at and the speed it follows at

        Configuring Displays
        a. On the right hand side of the display are buttons for selecting feeds, resizing, scaling, moving from a distance, and for HUDs snapping to the plane of the HUD.


Test Procedure
--------------

World Space Display Menu
1. Open the world space menu by selecting the display icon in under edit/Interface in the controller menu
	- Expected Behavior: Empty world space menu opens, all that is on it is the + icon to add new displays
2. Add a new display by selecting the + icon
	- Expected Behavior: A new display is added to the scene with no feed source connected to it
3. Add four more displays
	- Expected Behavior: The + icon should move to the middle center of the second row and each display should have a unique number
4. Dim a display by selecting the X on the bottom of the display
	- Expected Behavior: The display should dissapear (disabled)
5. Reopen the display by selecting the corresponding number in the world space display menu
	- Expected Behavior: The display should reappear in the same spot it was when it was disabled
6. Close the world space display menu by hitting close or the menu button on the controller
	- Expected Behavior: The menu should be hidden if done with the close or destroyed if done with the controller
7. Follow the display instructions below for some of the displays
8. Reopen the world space display menu by following step 1
	- Expected Behavior: The menu should open with the same displays on it as before

Heads-Up Display Menu
1. Open the Heads-Up Display Menu by selecting the HUD icon under edit/Interface 
	- Expected Behavior: Empty Heads-Up Display Menu should open on the load/save panel
2. Navigate to the edit submenu
	- Expected Behavior: The edit submenu should open.
3. Add a new display to the HUD by hitting the + icon.
	- Expected Behavior: A new display should appear in the mini HUD.
4. Push the display to the HUD by hitting the upload button
	- Expected Behavior: The same display should appear in the HUD, it should have no menus and not be interactable. It should not block the pointer
5. Toggle the HUD by hitting the checkmark button
	- Expected Behavior: the HUD should dissapear when disabled and reappear when enabled
6. Add a few more displays by following step 4 
	- Expected Behavior: More displays should appear on the HUD, each one slightly to the right
7. Follow the instructions below for testing displays.
8. Move the displays around to good positions then snap them to the HUD using the controller and the grid snap button on the display
	- Expected Behavior: Displays should be grabbable and move-able, when the snap button is hit any rotation should zero and on the plane
9. Push the new HUD to the HUD by following step 4.
	- Expected Behavior: The new HUD should overwrite the previous one.
10. Adjust the speed and threshold by navigating to the settings submenu and moving the slider
	- Expected Behavior: The HUD should follow at a different speed and start moving at a different threshold. If threshold set to zero and speed to max it should follow the headset directly
11. Save the HUD by navigating the the save panel, name it something and hit save.
	- Expected Behavior: The HUD should be saved 
12. Return to the editsubmenu and delete and add some components
13. Navigate to the load menu and load the HUD you just saved
14. Return to the editsubmenu
	- Expected Behavior: The previous HUD should be there with the same feeds as before
15. Close the HUD menu and reopen it and navigate to edit sub menu
	- Expected Behavior: The HUD should still be there

Display
1. Hit the settings icon on the top right
	- Expected Behavior: the settings menu should open below
For world space displays
2. Hit the pin icon
	- Expected Behavior: nothing should happen because it doesn't do anything.
For mini HUD displays
3. Hit the snap button (3x3 grid icon)
	- Expected Behavior: display should snap the plane of the HUD and rotation should be zeroed.
For all displays
4. Hit the feed button (download icon) and then open the scroll list and select a feed
	- Expected Behavior: A new feed should be shown on the display, if you select a HTML feed the resize button should grey out and be disabled
5. Hit the move icon, move the cursor off of the display and then back on and use the grip to move the display
	- Expected Behavior: Display should be move-able using the pointer
6. Hit the resize button and use both grips to expand or shrink the display along the x or y direction
	- Expected Behavior: Display should resize, note this doesn't work for HTML feeds and it skews the aspect ratio
7. Hit the scale button and use both grips to scale up or down the display
	- Expected Behavior: display should scale up or down
8. Hit the close button
	- Expected Behavior: display should close (for HUD displays it is deleted for worldspace it is dimmed)

Features
--------
- Customizable displays 
	- Support for in game video streams, sprites, HTML, video sources (youtube, local, livestreams)
	- Resize-able, scalable, move-able (from a distance)
- Configurable Heads-Up Display (HUD))
	- Menu for building HUDs in game
    - Loading and saving of HUDs
    - Closing displays in this mode will destroy them
- World space displays
    - Menu for adding displays to the world space
    - Closing displays in this mode will only hide them in the scene, not destroy them

Components
----------
- Display Prefab: The configurable display itself, the same prefab is used for both world space and HUD displays
    - DisplayController -> Handles the menu and the configuring of the display itself. Responsible for transforming the display into the different types of displays (World, Mini, HUD)
    - PanelSwitcher -> Handles the feed being displayed 
    - Universal Media Player -> Handles video streams
    - Under layout there is an HTMLPanel object as well as canvases, only one can be active at a time.

- DisplayMenu: Menu that handles the creation of world space displays, currently displays are not removed from the list when closed (only dimmed). A method exists in display to handle removing displays for the HUD, if desired this can be repurposed for the world space displays
    - DisplayMenu -> Script that handles the creation of world space displays, currently done in a poor way (menu is organized manually instead of using layouts) but it is functional.

- HUD: The object that holds the HUD itself, exists in the scene
    - FrameHUD -> Follows the headset FOV with adjustable offset threshold and following speed

- HudManager: Data manager for all things display, exists in the scene under modules
    - HudManager -> Stores the data from the HUDMenu and world space display lists, also keeps track of feed sources available and provides the link between the HUDMenu and the HUD.  

- HUDMenu: Menu that handles the creation of the HUD. Contains buttons to open up the OpenHUDPanel and SaveHUDPanel, also has sldiers to adjust the FrameHUD's speed and threshold. 
    - HudMenu -> Script that handles the mini HUD. Saves to the HudManager whenever navigated away from the EditSubMenu. Loads from the HudManager whenever navigated to the EditSubMenu. Contains methods to add new displays and push them to the HUD. 
    - EditSubMenu -> Script attached to the HUDMenu's editsubmenu, triggers loading and saving of the HUD.

- OpenHUDPanel: Menu for opening saved HUD's, once loaded the new HUD is in the EditSubMenu and needs to be pushed to the HUD.
    - HudOpenMenuManager -> Based off of the project open menu manager, newly saved HUDS are added everytime it is reopened.

- SaveHUDPanel: Menu for saving HUD's, enter a name and hit save. 
    - HudSaveMenuManager -> Based off of the project save menu manager, except it saves HUDs. NOTE: The HUD itself is not what is saved, rather it is the Mini HUD created in the HUDMenu.

- DisplayFeeds: Prefab that holds a bunch of pre-made display feeds.
    - FeedSource -> Script that holds the information to be displayed, must be in scene to be used.

Contribute
----------
Known Issues:
- Once an HTML display is pointed at, the blue UI laser pointer is hidden.
- Urls for the HTML display take ~30 seconds to load. If done manually it can happen immediately 
- Timing of youtube videos may be problematic (currently set to follow the timestamp of the first one played but it can be thrown off).
- Canvas sorting order is problematic, some things draw on top of others that they shouldn't
- World space displays block menu interaction


Future Work:
- Currently DisplayFeeds must be in the scene to be used and there is no way to create more once in game. Add a DisplayFeed creator menu.
- Add a save HUD option in addition to the existing save as.
- Add the ability to add displays to the users controllers.
- In the HUD prefab there is a deactivated Helmet object, this could eventually be replaced with a spacesuit helmet for astronaut training scenes
- Improve the UI for the speed and threshold submenu, add a numerical readout to show current status.


Support
-------

If you are having issues, please let me know.
You can reach me at rappaporttc@gmail.com

Or ask the resident Unity Expert, Dylan.

