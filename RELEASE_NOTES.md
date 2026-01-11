# Release Notes

## Version 1.1.0

Changes since v1.0.1

### New Features

- **Birthday Report by Group**: Allow user to select a group to pick birthday people from
- **Network Configuration**: Allow user to select to listen on localhost or 0.0.0.0
- **Installer**: Added creating an installer for easier distribution
- **Auto-start Browser**: In release mode, the application will start the browser automatically
- **Application Logo**: Introduced logo for Harmony

### Improvements

- **Reporting Page**: Works directly after startup; made reporting page easier to navigate/read
- **Live Preview**: Changing report column selection is immediately reflected in preview
- **Groups Column**: Added column for number of groups in Persons page
- **Search Focus**: Make sure search box has focus when navigating to people or groups page
- **F3 Shortcut**: F3 key now focuses the search textbox
- **Sorting**: Case-insensitive sorting of people and group names
- **Home Page**: Improved body of home page
- **UI Colors**: Improved color scheme
- **Textual Improvements**: Various textual improvements throughout the application

### Bug Fixes

- **Membership Modal**: Fix arrow buttons not working when editing memberships via groups page. Now using the DualListMembershipModal component, just like the persons page
- **Data Integrity**: Make sure memberships are deleted when a person or group is deleted
- **Orphan Cleanup**: On startup, delete membership records for people or groups that do not exist anymore

### Technical

- Installer creation now adds the application version number to the installer's filename
- Improved release procedure
- Write to log instead of console
- Changed/specified code's license
- Create GIT tag after successful installer creation
