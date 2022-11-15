using Godot;
using System;
using System.Threading.Tasks;

public class GeoCsharpWrapperTest : Control
{
    private HFlowContainer _testButtonContainer = null!;
    private RichTextLabel _locationDataOutput = null!;
    private RichTextLabel _errorOutput = null!;
    private Button _clearErrorOutput = null!;

    private Button _clearLocationOutput = null!;

    private TabContainer _tabContainer = null!;
    // private MenuButton _tabSelectMenuButton = null!;
    // private PopupMenu _tabSelectPopup = null!;
    private OptionButton _tabOptionButton = null!;

    public Location? CurrentLocation;

    public Location? StartLocation;
    public Location? EndLocation;

    public Geolocation GeolocationAPI = null!;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _tabContainer = GetNode<TabContainer>("%TestTabContainer");
        //_tabSelectMenuButton = GetNode<MenuButton>("%TabSelectMenuButton");
        //_tabSelectPopup = _tabSelectMenuButton.GetPopup();
        _tabOptionButton = GetNode<OptionButton>("%TabOptionButton");

        _testButtonContainer = GetNode<HFlowContainer>("%TestButtonContainer");
        _locationDataOutput = GetNode<RichTextLabel>("%LocationDataOutput");
        _errorOutput = GetNode<RichTextLabel>("%ErrorOutput");
        _clearErrorOutput = GetNode<Button>("%ClearErrorOutput");

        _clearLocationOutput = GetNode<Button>("%ClearLocationOutput");

        _clearErrorOutput.Connect("button_up", this, nameof(OnClearErrorOutput));
        _clearLocationOutput.Connect("button_up", this, nameof(OnClearLocationOutput));

        OnClearErrorOutput();
        Log("Ready");

        GeolocationAPI = GetNode<Geolocation>("/root/Geolocation");

        if (GeolocationAPI is null)
        {
            Log("Geolocation is null... node not found!");
            return;
        }

        if (GeolocationAPI.Supported)
        {
            Log("Geolocation supported");


            // Geolocation Settings
            GeolocationAPI.SetDebugLogSignal(true);
            GeolocationAPI.SetFailureTimeout(30);
            //GeolocationAPI.SetAutoCheckLocationCapability(true);
            //
        }
        else
        {
            Log("Geolocation NOT supported!");
        }

        GeolocationAPI.Log += OnLogEvent;
        GeolocationAPI.AuthorizationChanged += OnAuthorizationChangedEvent;
        //GeolocationAPI.LocationUpdate += OnLocationUpdateEvent;
        GeolocationAPI.Error += OnErrorEvent;

        //GeolocationAPI.HeadingUpdate += OnHeadingUpdate;

        InitUIAndWindowScaling();


        CreateTestTabsAndButtons();
        return;
        // calculate distance (test)
        //CreateDebugButton("Set Start", nameof(OnSetStartLocation));
        //CreateDebugButton("Set End", nameof(OnSetEndLocation));
        //CreateDebugButton("Calc Distance", nameof(OnShowDistance));
    }

    private void OnTabSelectPopupSelected(int id)
    {
        //Log("Selected Tab id: " + id);
        _tabContainer.CurrentTab = id;
    }

    private HFlowContainer CreateOneTab(string name)
    {
        var theTab = new HFlowContainer();
        theTab.Name = name;
        _tabContainer.AddChild(theTab);
        _tabOptionButton.AddItem(name);
        return theTab;
    }

    private void CreateTestTabsAndButtons()
    {
        _tabOptionButton.Connect("item_selected", this, nameof(OnTabSelectPopupSelected));

        Log("Creating Test Tabs");
        // permissions and authorization
        var permissionTab = CreateOneTab("Permissions");

        // request_permissions
        permissionTab.AddChild(ReturnButton("Request Permission Async", nameof(OnRequestPermissionAsync)));

        // location
        var locationTab = CreateOneTab("Location");

        // request_location
        locationTab.AddChild(ReturnButton("Request Location Async", nameof(OnRequestLocationAsync)));

        // start_updating_location
        locationTab.AddChild(ReturnButton("Start Updating Async", nameof(OnGetLocationUpdater)));

        // stop_updating_location
        locationTab.AddChild(ReturnButton("Stop Updating Async", nameof(OnStopLocationUpdater)));

        // heading
        var headingTab = CreateOneTab("Heading");

        // TODO make async!!!
        // start_updating_heading
        headingTab.AddChild(ReturnButton("Start heading", nameof(OnStartHeading)));
        // stop_updating_heading
        headingTab.AddChild(ReturnButton("Stop heading", nameof(OnStopHeading)));


        // options
        var optionsTab = CreateOneTab("Options");

        // set_distance_filter
        optionsTab.AddChild(ReturnButton("DistanceFilter 0", nameof(OnSetDistanceFilter), new Godot.Collections.Array { 0 }));
        optionsTab.AddChild(ReturnButton("DF 10", nameof(OnSetDistanceFilter), new Godot.Collections.Array { 10 }));
        optionsTab.AddChild(ReturnButton("DF 50", nameof(OnSetDistanceFilter), new Godot.Collections.Array { 50 }));

        // set_desired_accuracy
        optionsTab.AddChild(ReturnButton("Accuracy Best", nameof(OnSetDesiredAccuracyBest)));
        optionsTab.AddChild(ReturnButton("Ac 100m", nameof(OnSetDesiredAccuracyHundredMeters)));

        // set_update_interval
        optionsTab.AddChild(ReturnButton("Interval 1s", nameof(OnSetUpdateIntervalAndroid), new Godot.Collections.Array { 1 }));
        optionsTab.AddChild(ReturnButton("Interval 5s", nameof(OnSetUpdateIntervalAndroid), new Godot.Collections.Array { 5 }));

        // set_max_wait_time
        optionsTab.AddChild(ReturnButton("MaxWait 1s", nameof(OnSetMaxWaitTimeAndroid), new Godot.Collections.Array { 1 }));
        optionsTab.AddChild(ReturnButton("MaxWait 10s", nameof(OnSetMaxWaitTimeAndroid), new Godot.Collections.Array { 10 }));

        // set_return_string_coordinates
        // ignore... make private

        // set_failure_timeout
        optionsTab.AddChild(ReturnButton("Timeout 2s", nameof(OnSetFailureTimeout), new Godot.Collections.Array { 2 }));
        optionsTab.AddChild(ReturnButton("Timeout 20s", nameof(OnSetFailureTimeout), new Godot.Collections.Array { 20 }));

        // set_debug_log_signal
        // ignore...

        // status
        var statusTab = CreateOneTab("Status");

        // authorization_status
        statusTab.AddChild(ReturnButton("Auth. Status", nameof(OnGetAuthorizationStatus)));

        // allows_full_accuracy

        statusTab.AddChild(ReturnButton("Full Accuracy", nameof(OnCheckFullAccuracy)));

        // can_request_permissions

        statusTab.AddChild(ReturnButton("Can Request Perm.", nameof(OnCanRequestPermissions)));

        // is_updating_location  
        statusTab.AddChild(ReturnButton("Is Updating Loc.", nameof(OnCheckIsUpdatingLocation)));

        // is_updating_heading 
        statusTab.AddChild(ReturnButton("Is Updating Heading", nameof(OnCheckIsUpdatingHeading)));

        // should_show_permission_requirement_explanation
        statusTab.AddChild(ReturnButton("Should Explain", nameof(OnShouldShowPermissionRequirementExplanation)));

        // request_location_capabilty
        statusTab.AddChild(ReturnButton("Location Capability", nameof(OnCheckLocCap)));

        // Support checks
        var supportsTab = CreateOneTab("Supports");

        // supports_heading
        supportsTab.AddChild(ReturnButton("Heading", nameof(OnSupportsHeading)));

        // supports_update_interval
        supportsTab.AddChild(ReturnButton("Update Interval", nameof(OnSupportsUpdateInterval)));

        // supports_max_wait_time
        supportsTab.AddChild(ReturnButton("Max Wait Time", nameof(OnSupportsMaxWaitTime)));

        // supports_should_show_permission_requirement_explanation
        supportsTab.AddChild(ReturnButton("Should Explain", nameof(OnSupportsShouldExplain)));

        // supports_request_permissions
        supportsTab.AddChild(ReturnButton("Request Permisson", nameof(OnSupportsRequestPermission)));

        // Distance Calculator
        var utilityTab = CreateOneTab("Utility");

        utilityTab.AddChild(ReturnButton("Set Start", nameof(OnSetStartLocation)));
        utilityTab.AddChild(ReturnButton("Set End", nameof(OnSetEndLocation)));
        utilityTab.AddChild(ReturnButton("Show Distance", nameof(OnShowDistance)));

        _tabOptionButton.Selected = 0;
    }

    private void InitUIAndWindowScaling()
    {
        var currentOS = OS.GetName();
        Log(currentOS);

        var screenDpi = OS.GetScreenDpi();    
        Log("System DPI: " + screenDpi);
        float finalScaleFactor = 1;
        if(screenDpi > 140) finalScaleFactor = 1.51f;
        if(screenDpi > 250)
        {
            var scaleFactor = screenDpi / 150.0f;
            Log("raw scale factor: " + scaleFactor);
            finalScaleFactor = Mathf.FloorToInt(scaleFactor);
        }

        Log("ScaleFacor " + finalScaleFactor.ToString());
        GetTree().SetScreenStretch(SceneTree.StretchMode.Disabled, SceneTree.StretchAspect.Ignore, new Vector2(0, 0), finalScaleFactor);


        if (currentOS == "iOS" || currentOS == "Android") return; // don't scale window

        var currentSize = OS.WindowSize;
        OS.WindowSize = currentSize * finalScaleFactor;

        var screenSize = OS.GetScreenSize();

        OS.WindowPosition = (screenSize * 0.5f - OS.WindowSize * 0.5f);

        var colors = this.Theme.GetColorList("Button");
        GD.Print("Button colors:");
        Log("Button colors:");
        foreach (var color in colors)
        {
            GD.Print(color);
            Log(color);
        }

    }


    private void OnSupportsHeading() => Log("Supports heading " + GeolocationAPI.Supports("start_updating_heading"));
    private void OnSupportsUpdateInterval() => Log("Supports Update Interval " + GeolocationAPI.Supports("set_update_interval"));
    private void OnSupportsMaxWaitTime() => Log("Supports Max wait time " + GeolocationAPI.Supports("set_max_wait_time"));
    private void OnSupportsShouldExplain() => Log("Supports should explain " + GeolocationAPI.Supports("should_show_permission_requirement_explanation"));
    private void OnSupportsRequestPermission() => Log("Supports request poermisson " + GeolocationAPI.Supports("request_permission"));

    private async void OnRequestPermissionAsync()
    {
        var authorization = await GeolocationAPI.RequestPermissionsAsync();
        Log("OnRequestPermissionAsync " + authorization);
    }

    private async void OnCheckLocCap()
    {
        var capable = await GeolocationAPI.LocationCapabilityAsync();
        Log("OnCheckLocCap " + capable);
    }

    private async void OnRequestLocationAsync()
    {
        //Log("-- try auto location --");
        var loc = await GeolocationAPI.RequestLocationAsync();
        //Log("-- after await location --");
        if (loc != null)
        {
            Log("-- autolocation incoming --");
            LogLocation(loc);
            CurrentLocation = loc;
        }
        else
        {
            Log("Auto location null");
            Log("Last Error (might not be correct): " + GeolocationAPI.LastError);
        }


    }

    private Geolocation.LocationUpdater? _locationUpdater = null!;
    private Geolocation.HeadingUpdater? _headingUpdater = null!;

    private async void OnGetLocationUpdater()
    {
        if (_locationUpdater != null)
        {
            Log("## !!! stop old _locationUpdater");
            _locationUpdater.Stop();
        }

        _locationUpdater = await GeolocationAPI.GetLocationUpdater();

        if (_locationUpdater is null)
        {
            Log("## _locationUpdater null! don't loop");
            return;
        }

        Log("## entering new while loop");
        Location? loc;
        while ((loc = await _locationUpdater.LocationUpdateAsync()) != null)
        {
            LogLocation(loc);
            CurrentLocation = loc;
        }

        Log("#### YEAH after loop new"); // we should _always_ arrive here after updates stop
        if(_locationUpdater.HasError)
        {
            Log("Exited because of error: " + _locationUpdater.LastError.ToString());
        }

    }

    private void OnStopLocationUpdater()
    {
        if (_locationUpdater is null) return;

        _locationUpdater.Stop();
    }

    private void OnCanRequestPermissions()
    {
        Log("Can Request permissions: " + GeolocationAPI.CanRequestPermissions);
    }

    private void OnIsWatching()
    {
        Log("is watching: " + GeolocationAPI.IsUpdatingLocation);
    }

    private void OnLogEvent(object sender, string message)
    {
        Log(message);
    }

    private void OnAuthorizationChangedEvent(object sender, Geolocation.GeolocationAuthorizationStatus status)
    {
        Log("Auth Status changed to: " + status);
    }

    private void OnLocationUpdateEvent(object sender, Location locationData)
    {
        LogLocation(locationData);
        CurrentLocation = locationData;
    }

    private void OnErrorEvent(object sender, Geolocation.GeolocationErrorCodes error)
    {
        Log("ERROR: " + error.ToString());
    }



    ///////////////////////////////////// old ///////////////////////////////////////////////


    // is_updating_location
    private void OnCheckIsUpdatingLocation()
    {
        var isUpdating = GeolocationAPI.IsUpdatingLocation;
        Log("Location Updating: " + isUpdating);
    }

    private void OnCheckIsUpdatingHeading()
    {
        var isUpdating = GeolocationAPI.IsUpdatingHeading;
        Log("Heading Updating: " + isUpdating);
    }

    // should_show_permission_requirement_explanation
    private void OnShouldShowPermissionRequirementExplanation()
    {
        var shouldShow = GeolocationAPI.ShouldShowPermissionRequirementExplanation;
        Log("Should show Explanation: " + shouldShow);
    }


    private void OnSetFailureTimeout(int seconds)
    {
        GeolocationAPI.SetFailureTimeout(seconds);
    }

    private void OnSetMaxWaitTimeAndroid(int waitSeconds)
    {
        GeolocationAPI.SetMaxWaitTime(waitSeconds);
    }


    private void OnSetUpdateIntervalAndroid(int interval)
    {
        GeolocationAPI.SetUpdateInterval(interval);
    }

    private async void OnStartHeading()
    {
        /*
        if (GeolocationAPI.SupportsHeading)
        {
            GeolocationAPI.StartUpdatingHeading();
        }
        else
        {
            Log("Heading not supported!");
        }
        */
        if (_headingUpdater != null)
        {
            Log("## !!! stop old _headingUpdater");
            _headingUpdater.Stop();
        }

        _headingUpdater = GeolocationAPI.GetHeadingUpdater();

        if (_headingUpdater is null)
        {
            Log("## _headingUpdater null! don't loop");
            return;
        }

        Log("## entering new while loop");
        Heading? heading;
        while ((heading = await _headingUpdater.HeadingUpdateAsync()) != null)
        {
            LogHeading(heading);
        }

        Log("#### YEAH after heading loop new"); // we should _always_ arrive here after updates stop
    }

    private void OnStopHeading()
    {
        //GeolocationAPI.StopUpdatingHeading();
        if (_headingUpdater != null)
        {
            Log("#### Stopping heading updates");
            _headingUpdater.Stop();
        }
    }

    private void OnSetDesiredAccuracyBest()
    {
        GeolocationAPI.SetDesiredAccuracy(Geolocation.GeolocationDesiredAccuracyConstants.ACCURACY_BEST);
    }

    private void OnSetDesiredAccuracyHundredMeters()
    {
        GeolocationAPI.SetDesiredAccuracy(Geolocation.GeolocationDesiredAccuracyConstants.ACCURACY_HUNDRED_METERS);
    }

    private void OnSetDistanceFilter(float distance)
    {
        Log("try setting distance filter to: " + distance);
        GeolocationAPI.SetDistanceFilter(distance);
    }

    private void OnCheckFullAccuracy()
    {
        Log("Full accuracy is: " + GeolocationAPI.AllowsFullAccuracy);
    }

    private async void OnSetStartLocation()
    {
        StartLocation = await GeolocationAPI.RequestLocationAsync();
        if (StartLocation is null)
        {
            Log("Could not set Start Location!");
            return;
        }
        LogLocation(StartLocation, "Start Location");

    }

    private async void OnSetEndLocation()
    {
        EndLocation = await GeolocationAPI.RequestLocationAsync();
        if (EndLocation is null)
        {
            Log("Could not set End Location!");
            return;
        }


        LogLocation(EndLocation, "End Location");
    }

    private void OnShowDistance()
    {
        if (StartLocation is null || EndLocation is null)
        {
            Log("Start or end missing!");
            return;
        }

        Log("Distance in m: " + StartLocation.DistanceToMeters(EndLocation));
    }

    private void OnRequestLocation()
    {
        GeolocationAPI.RequestLocation();
    }

    private void OnGetAuthorizationStatus()
    {
        Log("Auth Status: " + GeolocationAPI.AuthorizationStatus);
    }

    private void LogLocation(Location location, string title = null!)
    {
        _locationDataOutput.Text = "";
        if (title != null) _locationDataOutput.Text += $"# {title} #\n";
        _locationDataOutput.Text += $"{location.ToString()}\n";
    }

    private void LogHeading(Heading heading, string title = null!)
    {
        _locationDataOutput.Text = "";
        if (title != null) _locationDataOutput.Text += $"# {title} #\n";
        _locationDataOutput.Text += $"{heading.ToString()}\n";
    }

    /*
    private void OnHeadingUpdate(object sender, Heading headingData)
    {
        //Log("got new heading");
        LogHeading(headingData);
    }
    */

    private void OnStartWatch()
    {
        Log("Start");
        GeolocationAPI.StartUpdatingLocation();
    }

    private void OnStopWatch()
    {
        Log("Stop");
        GeolocationAPI.StopUpdatingLocation();
    }
    private void OnRequestPermission()
    {
        Log("Requesting Permisson");
        GeolocationAPI.RequestPermissions();
    }


    private void OnClearErrorOutput()
    {
        _errorOutput.Text = "";
    }

    private void OnClearLocationOutput()
    {
        _locationDataOutput.Text = "";
    }

    private void Log(string message)
    {
        _errorOutput.Text = message + "\n" + _errorOutput.Text;
    }

    private void CreateDebugButton(string title, string nameOfMethod, Godot.Collections.Array paramArray = null!)
    {
        var target = this;

        var newBtn = new Button();
        newBtn.Text = title;
        newBtn.Connect("button_up", target, nameOfMethod, paramArray);

        _testButtonContainer.AddChild(newBtn);
    }

    private Button ReturnButton(string title, string nameOfMethod, Godot.Collections.Array paramArray = null!)
    {
        var target = this;

        var newBtn = new Button();
        newBtn.Text = title;
        newBtn.Connect("button_up", target, nameOfMethod, paramArray);

        return newBtn;
    }
}
