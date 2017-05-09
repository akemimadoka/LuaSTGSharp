Include'THlib.lua'

local _key_code_to_name={}
for k,v in pairs(KEY) do _key_code_to_name[v]=k end
for i=0,255 do _key_code_to_name[i]=_key_code_to_name[i] or '?' end
local setting_item={'res','windowed','vsync','sevolume','bgmvolume','lang'}

function save_setting()
	local f,msg
	f,msg=io.open('setting','w')
	if f==nil then
		error(msg)
	else
		f:write(Serialize(cur_setting))
		f:close()
	end
end
function setting_keys_default()
	cur_setting.keys.up=default_setting.keys.up
	cur_setting.keys.down=default_setting.keys.down
	cur_setting.keys.left=default_setting.keys.left
	cur_setting.keys.right=default_setting.keys.right
	cur_setting.keys.slow=default_setting.keys.slow
	cur_setting.keys.shoot=default_setting.keys.shoot
	cur_setting.keys.special=default_setting.keys.special
	cur_setting.keysys.repfast=default_setting.keysys.repfast
	cur_setting.keysys.repslow=default_setting.keysys.repslow
	cur_setting.keysys.menu=default_setting.keysys.menu
	cur_setting.keysys.snapshot=default_setting.keysys.snapshot
end
stage_init=stage.New('init',true,true)
function stage_init:init()
	--
	local f,msg
	f,msg=io.open('setting','r')
	if f==nil then
		cur_setting=DeSerialize(Serialize(default_setting))
	else
		cur_setting=DeSerialize(f:read('*a'))
		f:close()
	end
	--
	local function ExitGame()
		task.New(self,function()
			task.Wait(30)
			stage.QuitGame()
		end)
	end
	--
	New(mask_fader,'open')
	--
	--[[menu_title=New(simple_menu,'',{
		{'Start Game',function() save_setting()
				if lfs.attributes('.\\LuaSTGPlus.dev.exe')~=nil then
                    Execute("LuaSTGPlus.dev.exe","start_game=true")
				else
                    Execute("LuaSTGPlus.exe","start_game=true")
				end
				stage.QuitGame() end},
		{'Set User Name',function() menu.FlyIn(menu_name,'right') menu.FlyOut(menu_title,'left') end},
		{'Key Settings',function() menu.FlyIn(menu_key,'right') menu.FlyOut(menu_title,'left') menu_key.pos=1 end},
		{'Other Settings',function() menu.FlyIn(menu_other,'right') menu.FlyOut(menu_title,'left') menu_other.pos=1 end},
		{'Exit Launcher',ExitGame},
		{'exit',function() if menu_title.pos==5 then ExitGame() else menu_title.pos=5 end end},
	})]]
	--
	--menu_name=New(name_set_menu)
	--
	--menu_key=New(key_setting_menu,'Key Settings',{
	--	{'Up',function() menu_key.edit=true menu_key.setting_backup=cur_setting[setting_item[menu_key.pos]] end},
	--	{'Down',function() menu_key.edit=true menu_key.setting_backup=cur_setting[setting_item[menu_key.pos]] end},
	--	{'Left',function() menu_key.edit=true menu_key.setting_backup=cur_setting[setting_item[menu_key.pos]] end},
	--	{'Right',function() menu_key.edit=true menu_key.setting_backup=cur_setting[setting_item[menu_key.pos]] end},
	--	{'Slow',function() menu_key.edit=true menu_key.setting_backup=cur_setting[setting_item[menu_key.pos]] end},
	--	{'Shoot',function() menu_key.edit=true menu_key.setting_backup=cur_setting[setting_item[menu_key.pos]] end},
	--	{'Spell',function() menu_key.edit=true menu_key.setting_backup=cur_setting[setting_item[menu_key.pos]] end},
	--	{'Special',function() menu_key.edit=true menu_key.setting_backup=cur_setting[setting_item[menu_key.pos]] end},
	--	{'RepFast',function() menu_key.edit=true menu_key.setting_backup=cur_setting[setting_item[menu_key.pos]] end},
	--	{'RepSlow',function() menu_key.edit=true menu_key.setting_backup=cur_setting[setting_item[menu_key.pos]] end},
	--	{'Menu',function() menu_key.edit=true menu_key.setting_backup=cur_setting[setting_item[menu_key.pos]] end},
	--	{'SnapShot',function() menu_key.edit=true menu_key.setting_backup=cur_setting[setting_item[menu_key.pos]] end},
	--	{'Default',function() setting_keys_default() end},
	--	{'Return To Title',function() menu.FlyIn(menu_title,'left') menu.FlyOut(menu_key,'right') save_setting() end},
	--	{'exit',function()
	--		if menu_key.pos~=14 then
	--			menu_key.pos=14
	--		else
	--			menu.FlyIn(menu_title,'left')
	--			menu.FlyOut(menu_key,'right')
	--			save_setting()
	--		end
	--	end},
	--})
	--
	--[[menu_other=New(other_setting_menu,'Other Settings',{
		{'Resolution',function() end},
		{'Windowed',function() cur_setting.windowed=not cur_setting.windowed end},
		{'Vsync',function() cur_setting.vsync=not cur_setting.vsync end},
		{'Sound Volume',function() end},
		{'Music Volume',function() end},
		{'Language',function() end},
		{'Return To Title',function() menu.FlyIn(menu_title,'left') menu.FlyOut(menu_other,'right') save_setting() end},
		{'exit',function()
			if menu_other.pos~=7 then
				menu_other.pos=7
			else
				menu.FlyIn(menu_title,'left')
				menu.FlyOut(menu_other,'right')
				save_setting()
			end
		end},
	})
	--
	menu.FlyIn(menu_title,'right')]]
end
function stage_init:render()
	ui.DrawMenuBG()
end

name_set_menu=Class(object)

function name_set_menu:init()
	self.layer=LAYER_TOP
	self.group=GROUP_GHOST
	self.alpha=1
	self.x=screen.width*0.5-448
	self.y=screen.height*0.5-50
	self.bound=false
	self.posx=5
	self.posy=5
	self.pos_changed=0
	self.text=''
	self.forbidden={}
	self.title='Input User Name'
	self.locked=true
	self.text=cur_setting.username
end

function name_set_menu:frame()
	task.Do(self)
	if self.locked then return end
	if self.pos_changed>0 then self.pos_changed=self.pos_changed-1 end
	if GetLastKey()==setting.keys.up    then self.posy=self.posy-1 self.pos_changed=ui.menu.shake_time PlaySound('select00',0.3) end
	if GetLastKey()==setting.keys.down  then self.posy=self.posy+1 self.pos_changed=ui.menu.shake_time PlaySound('select00',0.3) end
	if GetLastKey()==setting.keys.left  then self.posx=self.posx-1 self.pos_changed=ui.menu.shake_time PlaySound('select00',0.3) end
	if GetLastKey()==setting.keys.right then self.posx=self.posx+1 self.pos_changed=ui.menu.shake_time PlaySound('select00',0.3) end
	self.posx=(self.posx+12)%12
	self.posy=(self.posy+ 8)% 8
	if KeyIsPressed'shoot' then
		if self.posx==11 and self.posy==7 then
			if  self.text=='' then self.text='User' end
			PlaySound('ok00',0.3)
			cur_setting.username=self.text
			menu.FlyIn(menu_title,'left')
			menu.FlyOut(menu_name,'right')
			save_setting()
			return
		end
		if #self.text==16 then
			self.posx=11 self.posy=7
		else
			local char=string.char(0x20+self.posy*12+self.posx)
			self.text=self.text..char
			PlaySound('ok00',0.3)
		end
	elseif KeyIsPressed'spell' then
		PlaySound('cancel00',0.3)
		if #self.text==0 then
			self.text='User'
			self.posx=11 self.posy=7
		else
			self.text=string.sub(self.text,1,-2)
		end
	end
end

function name_set_menu:render()
	SetFontState('menu','',Color(self.alpha*255,unpack(ui.menu.unfocused_color)))
	for posx=0,11 do
		for posy=0,7 do
			if posx~=self.posx or posy~=self.posy then
				RenderText('menu',string.char(0x20+posy*12+posx),self.x+(posx-5.5)*ui.menu.char_width,self.y-(posy-3.5)*ui.menu.line_height,ui.menu.font_size,'centerpoint')
			end
		end
	end
	local color={}
	local k=cos(self.timer*ui.menu.blink_speed)^2
	for j=1,3 do color[j]=ui.menu.focused_color1[j]*k+ui.menu.focused_color2[j]*(1-k) end
	SetFontState('menu','',Color(self.alpha*255,unpack(color)))
	RenderText('menu',string.char(0x20+self.posy*12+self.posx),
		self.x+(self.posx-5.5)*ui.menu.char_width+ui.menu.shake_range*sin(ui.menu.shake_speed*self.pos_changed),
		self.y-(self.posy-3.5)*ui.menu.line_height,
		ui.menu.font_size,'centerpoint')
	SetFontState('menu','',Color(self.alpha*255,unpack(ui.menu.title_color)))
	RenderText('menu',self.title,self.x,self.y+5.5*ui.menu.line_height,ui.menu.font_size,'centerpoint')
	RenderText('menu',self.text,self.x,self.y-5.5*ui.menu.line_height,ui.menu.font_size,'centerpoint')
end

local key_func={'up','down','left','right','slow','shoot','spell','special','repfast','repslow','menu','snapshot'}

key_setting_menu=Class(simple_menu)

function key_setting_menu:init(title,content)
	simple_menu.init(self,title,content)
	self.w=20
end

function key_setting_menu:frame()
	task.Do(self)
	if self.locked then return end
	if self.pos_changed>0 then self.pos_changed=self.pos_changed-1 end
	local last_key=GetLastKey()
	if last_key~=KEY.NULL then
		local item=setting_item[self.pos]
		self.pos_changed=ui.menu.shake_time
		if self.pos<=12 then
			if self.edit then
				GetLastKey()
				if self.pos<=8 then
					cur_setting.keys[key_func[self.pos]]=last_key
				elseif self.pos<=12 then
					cur_setting.keysys[key_func[self.pos]]=last_key
				end
				self.edit=false
				save_setting()
				return
			end
		end
--		self.pos=self.pos+1
--		if self.pos==13 then
--			self.pos=12
--			menu.FlyIn(menu_title,'left')
--			menu.FlyOut(menu_key,'right')
	end

	if not self.edit then simple_menu.frame(self) end
end

function key_setting_menu:render()
	SetFontState('menu','',Color(self.alpha*255,unpack(ui.menu.title_color)))
	RenderText('menu',self.title,self.x,self.y+ui.menu.line_height*6.5,ui.menu.font_size,'centerpoint')
	ui.DrawMenu('',self.text,self.pos,self.x-128,self.y-ui.menu.line_height,self.alpha,self.timer,self.pos_changed,'left')
	local key_name={}
	if self.edit then
		if self.timer%30<15 then
			RenderText('menu','___',self.x+128,self.y+ui.menu.line_height*(7-self.pos),ui.menu.font_size,'right')
		end
	end
	for i=1,8 do
		table.insert(key_name,_key_code_to_name[cur_setting.keys[key_func[i]]])
	end
	for i=9,12 do
		table.insert(key_name,_key_code_to_name[cur_setting.keysys[key_func[i]]])
	end
	table.insert(key_name,'')
	table.insert(key_name,'')
	ui.DrawMenu('',key_name,self.pos,self.x+128,self.y-ui.menu.line_height,self.alpha,self.timer,self.pos_changed,'right')
end

other_setting_menu=Class(simple_menu)

function other_setting_menu:init(title,content)
	simple_menu.init(self,title,content)
	self.w=24
	self.posx=1
end

function other_setting_menu:frame()
	task.Do(self)
	if self.locked then return end
	local last_key=GetLastKey()
	if last_key~=KEY.NULL then
		local item=setting_item[self.pos]
		if self.pos==6 then
			if last_key==setting.keys.left then
				cur_setting[item]=max(1, cur_setting[item]-1)
				PlaySound('select00',0.3)
			elseif last_key==setting.keys.right then
				cur_setting[item]=min(#LANG, cur_setting[item]+1)
				PlaySound('select00',0.3)
			end
		elseif self.pos>=4 then
			if last_key==setting.keys.left then cur_setting[item]=max(0,cur_setting[item]-1) PlaySound('select00',0.003*cur_setting[item])
			elseif last_key==setting.keys.right then cur_setting[item]=min(100,cur_setting[item]+1) PlaySound('select00',0.003*cur_setting[item]) end
		elseif self.pos==1 then
			if last_key==setting.keys.left then
				cur_setting[item]=max(2, cur_setting[item]-1)
				PlaySound('select00',0.3)
			elseif last_key==setting.keys.right then
				cur_setting[item]=min(#RES, cur_setting[item]+1)
				PlaySound('select00',0.3)
			end
		else
			if last_key==setting.keys.left or last_key==setting.keys.right then
				cur_setting[item]=not cur_setting[item]
				PlaySound('select00',0.3)
			end
		end
	end
	if not self.edit then simple_menu.frame(self) end
end

function other_setting_menu:render()
	SetFontState('menu','',Color(self.alpha*255,unpack(ui.menu.title_color)))
	RenderText('menu',self.title,self.x,self.y+ui.menu.line_height*4,ui.menu.font_size,'centerpoint')
	if self.pos<=2 and self.edit then
		if self.timer%30<15 then
			RenderText('menu','_',self.x+128-(self.posx-1)*ui.menu.num_width,self.y+ui.menu.line_height*(3.5-self.pos),ui.menu.font_size,'right')
		end
	end
	ui.DrawMenu('',self.text,self.pos,self.x-128,self.y-ui.menu.line_height,self.alpha,self.timer,self.pos_changed,'left')
	local setting_text={}
	local cur_res=RES[cur_setting[setting_item[1]]]
	setting_text[1]=tostring(cur_res.x..'x'..cur_res.y)
	for i=2,5 do
		setting_text[i]=tostring(cur_setting[setting_item[i]])
	end
	setting_text[6]=tostring(LANG[cur_setting[setting_item[6]]])
	setting_text[7]=''
	ui.DrawMenu('',setting_text,self.pos,self.x+128,self.y-ui.menu.line_height,self.alpha,self.timer,self.pos_changed,'right')
end
