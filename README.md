# uwp-demo
Demo project to show inking capabilities in UWP (Universal Windows Plattform) apps. This demo app provides a whiteboard which can display images as a background. The users can draw annotations to this image by using inking features via stylus, touch or mouse. There is a sync feature which allows to start the app on multiple devices and transfer the inputs of all users to all other instances of the app which allows remote sessions.

This app is used as a demo for a workshop for developing for the Microsoft Surface Hub provided by artiso (www.artiso.com) for Microsoft. 

## Testing with own service
To setup your own service you can run it locally or publish it as aa Azure App Service. There is also a table storage used for storing some information about sessions. This has to be configured by the connection string to the storage account in the web.conf or via the app settings in the App Service.