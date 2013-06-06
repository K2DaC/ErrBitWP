Errbit Notifier for Windows Phone
========


The Errbit Notifier for Windows Phone is designed to send a report of any uncaught exception thrown from your Windows Phone application.

put this line to the constructor of your App.cs. Make sure it's the last line there!

 
```
 ErrBitNotify.ErrBitNotify.Register("API-KEY", "ENDPOINT", this);
