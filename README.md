Errbit Notifier for Windows Phone
========


The Errbit Notifier for Windows Phone is designed to give you instant notification of any uncaught exceptions thrown from your Windows Phone applications.

put this line to your App.cs

 
```
 private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
 {
               ErrBitNotify.ErrBitNotify.Register("API KEY", "URL", e.ExceptionObject);
            
 }
