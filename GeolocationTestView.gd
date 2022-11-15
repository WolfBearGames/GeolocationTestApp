extends Control


onready var test_button_container:HFlowContainer = $"%TestButtonContainer"
onready var location_data_output:RichTextLabel = $"%LocationDataOutput"
onready var error_output:RichTextLabel = $"%ErrorOutput"
onready var clear_error_output:Button = $"%ClearErrorOutput"
onready var clear_location_output:Button = $"%ClearLocationOutput"

var geolocation_api:GeolocationWrapper

var location_watcher:LocationWatcher

# Called when the node enters the scene tree for the first time.
func _ready():
	clear_error_output.connect("button_up",self,"on_clear_error")
	clear_location_output.connect("button_up",self,"on_clear_location")
	on_clear_error()
	
	create_debug_button("Request Location","_on_button_request_location")
	
	create_debug_button("Start Updates","_on_button_start_location_updates")
	create_debug_button("Stop Updates","_on_button_stop_location_updates")
	
	create_debug_button("Disable Plugn debug","_on_button_disable_debug_messages")
	
	geolocation_api= get_node("/root/GeolocationWrapper")
	glog("Geolocation supported: " + geolocation_api.supported as String)

	if geolocation_api.supported:
		_register_signals()
		geolocation_api.set_failure_timeout(20) # remove
		geolocation_api.set_debug_log_signal(true)
		#geolocation_api.request_location_capabilty()
 

func _register_signals():
	glog("registering signals")
	geolocation_api.connect("authorization_changed", self, "_on_authorization_changed")
	geolocation_api.connect("error", self, "_on_error")
	geolocation_api.connect("debug", self, "_on_debug")
	geolocation_api.connect("location_update", self, "_on_location_update")
	geolocation_api.connect("heading_update", self, "_on_heading_update")
	

func on_clear_error():
	error_output.text = ""
	
func on_clear_location():
	location_data_output.text = ""
	
func create_debug_button(title:String, method:String, parameters:Array = []):
	var newButton:Button = Button.new()
	newButton.text = title
	newButton.connect("button_up",self,method, parameters)
	test_button_container.add_child(newButton)
	
	
func _on_authorization_changed(status:int):
	glog("+signal authorization changed: " + status as String)

func _on_error(code:int):
	glog("Error: " + str(code))
	#glog("+signal ERROR: " + geolocation_api.geolocation_error_codes.keys()[code-1] + "(" + str(code) + ")")
	
func _on_debug(message :String, number:float = 0):
	glog("log: " + message + "(" + number as String + ")")
	
func _on_location_update(location:Location):
	glog("+signal location update!")
	#set_location_output(location.to_string())
	
func _on_heading_update(heading_data:Dictionary):
	glog("+signal heading update!")
	
	
func glog(message:String):
	error_output.text = message + "\n" + error_output.text
	
func set_location_output(content:String):
	location_data_output.text = content


# called when "Request Location" button is pressed
func _on_button_request_location():
	var request = geolocation_api.request_location_autopermission()
	var location:Location = yield(request,"location_update")
	glog("after yield")
	# location is null when no location could be found (no permission, no connection, no capabilty)
	if location == null:
		glog("location was null")
		# log error if an error was reported
		if request.error > 0:
			set_location_output("Error: " + str(request.error))
			#set_location_output("Error: " + geolocation_api.geolocation_error_codes.keys()[request.error-1])
		return
	# show location 
	location_data_output.text = location.to_string()

# start updating location button pressed
func _on_button_start_location_updates():
	# stop old watcher
	if location_watcher != null && location_watcher.is_updating:
		location_watcher.stop()
	
	# create watcher and wait for ready
	location_watcher = geolocation_api.start_updating_location_autopermission()
	var success:bool = yield(location_watcher, "ready")
	
	# report error
	if !success:
		# log error if an error was reported
		if location_watcher.error > 0:
			set_location_output("Error: " + str(location_watcher.error))
		return
	
	# wait for new location in loop until stopped
	while(location_watcher.is_updating):
		var location:Location = yield(location_watcher, "location_update")
		if location == null:
			set_location_output("Error: location null where it should never be null")
			continue
		location_data_output.text = location.to_string()
		
	glog("after watching while loop. should be end here after stop or error")

# stop updating location button pressed
func _on_button_stop_location_updates():
	if location_watcher != null:
		location_watcher.stop()	
		
func _on_button_disable_debug_messages():
	geolocation_api.set_debug_log_signal(false)
