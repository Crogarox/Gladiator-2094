using Godot;
using System;

public class PlayerGun : Gun
{
	/*Currently, Godot engine in its vanilla form 
	does not have an ideal amount of support for custom classes, data types, etc.
	Ideally, weapons would be a struct or something to contain their 
	firing sound, cooldown time, etc, but that isnt possible.
	The engine developers have a solution in the works, 
	but for now, we have this utterly riddiculous spaghetti code mess*/
	//GAMEPLAY VARIABLES-----------------------------------------------------------
	[Export]
	public int shells, bullets, grenades;
	[Export]
	public float coolSpeed = 3f, buckshotSpread = 0.06f, recoilAnim = 0.002f, recoilOffset = -0.722f;
	[Export]
	public float coolRepeater, coolBuckshot, coolGrenades, coolFlames;
	//COMPONENT VARIABLES------------------------------------------------------------
	PackedScene instanceRepeater = 
	(PackedScene)ResourceLoader.Load("res://Prefabs/Bullets/Player_Repeater.tscn");
	PackedScene instanceBuckshot = 
	(PackedScene)ResourceLoader.Load("res://Prefabs/Bullets/Player_Buckshot.tscn");
	PackedScene instanceGrenade = 
	(PackedScene)ResourceLoader.Load("res://Prefabs/Bullets/Player_Grenades.tscn");
	PackedScene instanceFlame = 
	(PackedScene)ResourceLoader.Load("res://Prefabs/Bullets/Player_Flame.tscn");
	[Export]
	public NodePath HUDPath;
	[Export]
	public NodePath PlayerPath;
	public Player player;
	public TextureProgress coolMeter;
	public TextureRect iconRptr, iconShot, iconGren, iconFlam; 
	private Label ammoNum;
	public AudioStreamPlayer pickupSnd, switchSnd;
	public AudioStreamSample fireGren = 
	(AudioStreamSample)ResourceLoader.Load("res://Sounds/guns/grenadeLaunch.wav"),
	fireRep = (AudioStreamSample)ResourceLoader.Load("res://Sounds/guns/repeater.wav"), 
	fireShot = (AudioStreamSample)ResourceLoader.Load("res://Sounds/guns/buckshot.wav"), 
	wepSwitch = (AudioStreamSample)ResourceLoader.Load("res://Sounds/guns/switch.wav");

	public AudioStreamSample pickupGren = 
	(AudioStreamSample)ResourceLoader.Load("res://Sounds/pickups/grenadePickup.wav"),
	pickupRep = (AudioStreamSample)ResourceLoader.Load("res://Sounds/pickups/repeaterPickup.wav"), 
	pickupShot = (AudioStreamSample)ResourceLoader.Load("res://Sounds/pickups/buckshotPickup.wav");
	public bool disabled = false;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		coolMeter = GetNode<TextureProgress>("Meter/Viewport/TextureProgress");
		player = GetNode<Player>(PlayerPath);
		iconRptr = GetNode<TextureRect>(HUDPath + "/WeaponBar/HBoxContainer/IconRptr");
		iconShot = GetNode<TextureRect>(HUDPath + "/WeaponBar/HBoxContainer/IconShot");
		iconGren = GetNode<TextureRect>(HUDPath + "/WeaponBar/HBoxContainer/IconGren");
		iconFlam = GetNode<TextureRect>(HUDPath + "/WeaponBar/HBoxContainer/IconFlam");
		ammoNum = GetNode<Label>(HUDPath + "/FuelMeter/AmmoNum");
		pickupSnd = GetNode<AudioStreamPlayer>("pickupSnd");
		switchSnd = GetNode<AudioStreamPlayer>("switchSnd");
		currentWeapon = Weapons.FLAMETHROWER;
		UpdateWeaponData();
	}

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		coolMeter.Value -= 50 * coolSpeed * delta;
		Translation = new Vector3 (Translation.x, Translation.y, (recoilAnim * (float)coolMeter.Value) + recoilOffset);
		ProcessInput(delta);
		if (currentWeapon == Weapons.FLAMETHROWER)
		{
			int fuelInt = (int)player.fuel;
			ammoNum.Text = fuelInt.ToString();
		}
		
	}
    /*public override void _PhysicsProcess(float delta)
    {
		
		//no idea why this needs to happen in _PhysicsProcess, 
		//but the meter jams if I decrement it by coolSpeed * delta.
		//and no other timers in the game malfunction when using delta but this one???
    }*/
    
	protected void ProcessInput(float delta)
	{
		if (coolMeter.Value == 0  && !disabled)
		{
			//Which weapon do I fire?
			if (Input.IsActionPressed("player_fire1"))
			{
				switch (currentWeapon)
				{
					case (Weapons.REPEATER):
						FireRepeater();
					break;
					case (Weapons.BUCKSHOT):
						FireBuckshot();
					break;
					case (Weapons.GRENADES):
						FireGrenades();
					break;
					case (Weapons.FLAMETHROWER):
						FireFlame();
					break;
				}
			}
		}
		//FLAMETHROWER RECHARGE MANAGEMENT
		if ((Input.IsActionPressed("player_fire1") && currentWeapon == Weapons.FLAMETHROWER))
		{
			player.flameThrowerOn = true;
		}
		else player.flameThrowerOn = false;
		
	}
	
	protected void FireRepeater()//this one is gonna need extra parameters when we add multiple weapons
	{
		if (bullets > 0)
		{
			switchSnd.Stream = fireRep;
			switchSnd.Play();
			coolMeter.Value = 100;
			bullets--;
			ammoNum.Text = bullets.ToString();
			
			Bullet newBullet = (Bullet)instanceRepeater.Instance();
			newBullet.Transform = GlobalTransform * barrels[0];
			GetTree().Root.AddChild(newBullet);
			newBullet.ApplyImpulse(new Vector3(0, 0, 0), -newBullet.GlobalTransform.basis.z * newBullet.speed);
		}
		
	}
	protected void FireBuckshot()//this one is why I gave the player 9 barrel transforms
	{   
		if (shells > 0)
		{
			switchSnd.Stream = fireShot;
			switchSnd.Play();
			coolMeter.Value = 100;
			shells--;
			ammoNum.Text = shells.ToString();
			for (int i = 0; i < barrels.Count; i++)
            {
				Bullet newBullet = (Bullet)instanceBuckshot.Instance();
				newBullet.Transform = GlobalTransform * barrels[i];
				GetTree().Root.AddChild(newBullet);
				newBullet.ApplyImpulse(new Vector3(0, 0, 0), -newBullet.GlobalTransform.basis.z * newBullet.speed);
            }
			
		}
	}
	protected void FireGrenades()
	{
		if (grenades > 0)
		{
			switchSnd.Stream = fireGren;
			switchSnd.Play();
			coolMeter.Value = 100;
			grenades--;
			ammoNum.Text = grenades.ToString();
			
			Bullet newBullet = (Bullet)instanceGrenade.Instance();
			newBullet.Transform = GlobalTransform * barrels[0];
			GetTree().Root.AddChild(newBullet);
			newBullet.ApplyImpulse(new Vector3(0, 0, 0), -newBullet.GlobalTransform.basis.z * newBullet.speed);
		}
	}
	protected void FireFlame()
	{ /*Flames aren't done using the particle system because Godot does not support per-particle collission
		so what we have instead is an extremely rapid fire bullet with short lifespan, effectively limiting its range */
		if (player.fuel > 0)
		{
			coolMeter.Value = 100;
			player.fuel--;
			int fuelInt = (int)player.fuel;
			ammoNum.Text = fuelInt.ToString();
			Bullet newBullet = (Bullet)instanceFlame.Instance();
			newBullet.Transform = GlobalTransform * barrels[0];
			//newBullet.Translate(barrels[0].origin);
			GetTree().Root.AddChild(newBullet);
			newBullet.ApplyImpulse(new Vector3(0, 0, 0), -newBullet.GlobalTransform.basis.z * newBullet.speed);
		}
	}
	public void AddBullets(int delta)
	{
		pickupSnd.Stream = pickupRep;
		pickupSnd.Play();
		bullets += delta;
		bullets = Mathf.Clamp(bullets, 0, 999);
		UpdateWeaponData();
	}
	public void AddShells(int delta)
	{
		pickupSnd.Stream = pickupShot;
		pickupSnd.Play();
		shells += delta;
		shells = Mathf.Clamp(shells, 0, 999);
		UpdateWeaponData();
	}
	public void AddGrenades(int delta)
	{
		pickupSnd.Stream = pickupGren;
		pickupSnd.Play();
		grenades += delta;
		grenades = Mathf.Clamp(grenades, 0, 999);
		UpdateWeaponData();
	}
	public void NextWeapon()
	{
		if ((byte)currentWeapon >= 4)
		{
			currentWeapon = Weapons.FLAMETHROWER;
		}
		else currentWeapon++;
		switchSnd.Stream = wepSwitch;
		switchSnd.Play();
		UpdateWeaponData();
	}
	public void PrevWeapon()
	{
		if ((byte)currentWeapon <= 1)
		{
			currentWeapon = Weapons.GRENADES;
		}
		else currentWeapon--;
		switchSnd.Stream = wepSwitch;
		switchSnd.Play();
		UpdateWeaponData();
	}
	void UpdateWeaponData()
	{
		switch (currentWeapon)
		{
			case (Weapons.REPEATER):
			iconRptr.Modulate = new Color(1,1,1,1);
			iconGren.Modulate = new Color(1,1,1,0.5f);
			iconShot.Modulate = new Color(1,1,1,0.5f);
			iconFlam.Modulate = new Color(1,1,1,0.5f);
			coolSpeed = coolRepeater;
			ammoNum.Text = bullets.ToString();
			
			break;
			case (Weapons.BUCKSHOT):
			iconRptr.Modulate = new Color(1,1,1,0.5f);
			iconGren.Modulate = new Color(1,1,1,0.5f);
			iconShot.Modulate = new Color(1,1,1,1);
			iconFlam.Modulate = new Color(1,1,1,0.5f);
			coolSpeed = coolBuckshot;
			ammoNum.Text = shells.ToString();
			break;
			case (Weapons.GRENADES):
			iconRptr.Modulate = new Color(1,1,1,0.5f);
			iconGren.Modulate = new Color(1,1,1,1);
			iconShot.Modulate = new Color(1,1,1,0.5f);
			iconFlam.Modulate = new Color(1,1,1,0.5f);
			coolSpeed = coolGrenades;
			ammoNum.Text = grenades.ToString();
			break;
			case (Weapons.FLAMETHROWER):
			iconRptr.Modulate = new Color(1,1,1,0.5f);
			iconGren.Modulate = new Color(1,1,1,0.5f);
			iconShot.Modulate = new Color(1,1,1,0.5f);
			iconFlam.Modulate = new Color(1,1,1,1);
			coolSpeed = coolFlames;
			int fuelInt = (int)player.fuel;
			ammoNum.Text = fuelInt.ToString();
			break;
				
		}
	}
	public override void _Input(InputEvent @event){
		if (coolMeter.Value == 0  && !disabled)
		{
			if  (@event.IsActionReleased("player_nextWep"))
			{
				NextWeapon();
			}
			else if  (@event.IsActionReleased("player_prevWep"))
			{
				PrevWeapon();
			}
		}
		if (@event is InputEventKey eventKey)
		{
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Key1)
			{
				currentWeapon = Weapons.FLAMETHROWER;
				switchSnd.Stream = wepSwitch;
				switchSnd.Play();
				UpdateWeaponData();
			}
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Key2)
			{
				currentWeapon = Weapons.REPEATER;
				switchSnd.Stream = wepSwitch;
				switchSnd.Play();
				UpdateWeaponData();
			}
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Key3)
			{
				currentWeapon = Weapons.BUCKSHOT;
				switchSnd.Stream = wepSwitch;
				switchSnd.Play();
				UpdateWeaponData();
			}
			if (eventKey.Pressed && eventKey.Scancode == (int)KeyList.Key4)
			{
				currentWeapon = Weapons.GRENADES;
				switchSnd.Stream = wepSwitch;
				switchSnd.Play();
				UpdateWeaponData();
			}
		}      
	}
}
