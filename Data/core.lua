GROUP_GHOST=lstg.GetCollisionLayerId("Default");
GROUP_ENEMY_BULLET=lstg.GetCollisionLayerId("Bullet");
GROUP_ENEMY=lstg.GetCollisionLayerId("Enemy");
GROUP_PLAYER_BULLET=lstg.GetCollisionLayerId("PlayerBullet");
GROUP_PLAYER=lstg.GetCollisionLayerId("Player");
GROUP_ITEM=lstg.GetCollisionLayerId("Item");
--GROUP_ALL=16
GROUP_NUM_OF_GROUP=16

LAYER_BG=-700
LAYER_ENEMY=-600
LAYER_PLAYER_BULLET=-500
LAYER_PLAYER=-400
LAYER_ITEM=-300
LAYER_ENEMY_BULLET=-200
LAYER_ENEMY_BULLET_EF=-100
LAYER_TOP=0

PI=math.pi
PIx2=math.pi*2
PI_2=math.pi*0.5
PI_4=math.pi*0.25
SQRT2=math.sqrt(2)
SQRT3=math.sqrt(3)
SQRT2_2=math.sqrt(0.5)
GOLD=360*(math.sqrt(5)-1)/2

function RawDel(o)
	if o then
		o.status='del'
		if o._servants then _del_servants(o) end
	end
end
function RawKill(o)
	if o then
		o.status='kill'
		if o._servants then _kill_servants(o) end
	end
end
function PreserveObject(o)
	o.status='normal'
end
do
	local old = Kill
	function Kill(o)
		if o then
			if o._servants then _kill_servants(o) end
			old(o)
		end
	end
end
do
	local old = Del
	function Del(o)
		if o then
			if o._servants then _del_servants(o) end
			old(o)
		end
	end
end

int=math.floor
abs=math.abs
max=math.max
min=math.min
mod=math.mod
rnd=math.random
sqrt=math.sqrt
function sign(x) if x>0 then return 1 elseif x<0 then return -1 else return 0 end end
ran=Rand()
function hypot(x,y) return sqrt(x*x+y*y) end

function SetV2(obj,v,angle,rot,aim)
	if aim then SetV(obj,v,angle+Angle(obj,lstg.player),rot) else SetV(obj,v,angle,rot) end
end

function FileExist(filename)
	return not (lfs.attributes(filename)==nil)
end

lstg.included={}
lstg.current_script_path={''}
function Include(filename)
	filename=tostring(filename)
	if string.sub(filename,1,1)=='~' then
		filename=lstg.current_script_path[#lstg.current_script_path]..string.sub(filename,2)
	end
	if not lstg.included[filename] then
		local i,j=string.find(filename,'^.+[\\/]+')
		if i then table.insert(lstg.current_script_path,string.sub(filename,i,j)) else table.insert(lstg.current_script_path,'') end
		lstg.included[filename]=true
		DoFile(filename)
		lstg.current_script_path[#lstg.current_script_path]=nil
	end
end

ImageList = {}
OriginalLoadImage = LoadImage
function LoadImage(img,...)
	local arg = { ... }
	ImageList[img] = arg
	OriginalLoadImage(img,...)
end

function CopyImage(newname,img)
	if ImageList[img] then
		LoadImage(newname,unpack(ImageList[img]))
	elseif img then
		error("The image \""..img.."\" can't be copied.")
	else
		error("Wrong argument #2 (expect string get nil)")
	end
end

function LoadImageGroup(prefix,texname,x,y,w,h,cols,rows,a,b,rect)
	for i=0,cols*rows-1 do
		--[[lstg.]]LoadImage(prefix..(i+1),texname,x+w*(i%cols),y+h*(int(i/cols)),w,h,a or 0,b or 0,rect or false)
	end
end

function LoadImageFromFile(teximgname,filename,mipmap,a,b,rect)
	LoadTexture(teximgname,filename,mipmap)
	local w,h=GetTextureSize(teximgname)
	LoadImage(teximgname,teximgname,0,0,w,h,a or 0,b or 0,rect)
end

function LoadAniFromFile(texaniname,filename,mipmap,n,m,intv,a,b,rect)
	LoadTexture(texaniname,filename,mipmap)
	local w,h=GetTextureSize(texaniname)
	LoadAnimation(texaniname,texaniname,0,0,w/n,h/m,n,m,intv,a,b,rect)
end

function LoadImageGroupFromFile(texaniname,filename,mipmap,n,m,a,b,rect)
	LoadTexture(texaniname,filename,mipmap)
	local w,h=GetTextureSize(texaniname)
	LoadImageGroup(texaniname,texaniname,0,0,w/n,h/m,n,m,a,b,rect)
end

ENUM_RES_TYPE={tex=1,img=2,ani=3,bgm=4,snd=5,psi=6,fnt=7,ttf=8}
function CheckRes(typename,resname)
	local t=ENUM_RES_TYPE[typename]
	if t==nil then error('Invalid resource type name.')
	else return lstg.CheckRes(t,resname) end
end
function EnumRes(typename)
	local t=ENUM_RES_TYPE[typename]
	if t==nil then error('Invalid resource type name.')
	else return lstg.EnumRes(t) end
end

screen={}
if RES[setting.res].x>RES[setting.res].y then
	screen.width=640
	screen.height=480
	screen.scale=RES[setting.res].y/screen.height
	screen.dx=(RES[setting.res].x-screen.scale*screen.width)*0.5
	screen.dy=0
	lstg.world={l=-192,r=192,b=-224,t=224,boundl=-224,boundr=224,boundb=-256,boundt=256,scrl=32,scrr=416,scrb=16,scrt=464}
else
	screen.width=396
	screen.height=528
	screen.scale=RES[setting.res].x/screen.width
	screen.dx=0
	screen.dy=(RES[setting.res].y-screen.scale*screen.height)*0.5
	lstg.world={l=-192,r=192,b=-224,t=224,boundl=-224,boundr=224,boundb=-256,boundt=256,scrl=6,scrr=390,scrb=16,scrt=464}
end
--SetBound(lstg.world.boundl,lstg.world.boundr,lstg.world.boundb,lstg.world.boundt)

function WorldToScreen(x,y)
	local w=lstg.world
	return w.scrl+(w.scrr-w.scrl)*(x-w.l)/(w.r-w.l),w.scrb+(w.scrt-w.scrb)*(y-w.b)/(w.t-w.b)
end

ENUM_TTF_DECO={italic=1,underline=2,strikeout=4}
setmetatable(ENUM_TTF_DECO,{__index=function(t,k) return 0 end})
function LoadTTF(ttfname,filename,size)
	lstg.LoadTTF(ttfname,filename,0,size*screen.scale)
end

ENUM_TTF_FMT={
	left=0x00000000,
	center=0x00000001,
	right=0x00000002,
	top=0x00000000,
	vcenter=0x00000004,
	bottom=0x00000008,
	wordbreak=0x00000010,
	singleline=0x00000020,
	expantextabs=0x00000040,
	noclip=0x00000100,
	calcrect=0x00000400,
	rtlreading=0x00020000,
	paragraph=0x00000010,
	centerpoint=0x00000105,
}
setmetatable(ENUM_TTF_FMT,{__index=function(t,k) return 0 end})
function RenderTTF(ttfname,text,left,right,bottom,top,color,...)
	local fmt=0
	local arg = { ... }
	for i=1,#arg do fmt=fmt+ENUM_TTF_FMT[arg[i]] end
	lstg.RenderTTF(ttfname,text,left,right,bottom,top,fmt,color)
end
function RenderText(fontname,text,x,y,size,...)
	local fmt=0
	local arg = { ... }
	for i=1,#arg do fmt=fmt+ENUM_TTF_FMT[arg[i]] end
	lstg.RenderText(fontname,text,x,y,size,fmt)
end

function new_scoredata_table()
	t={}
	setmetatable(t,{__newindex=scoredata_mt_newindex,__index=scoredata_mt_index,data={}})
	return t
end
function scoredata_mt_newindex(t,k,v)
	if type(k)~='string' and type(k)~='number' then error('Invalid key type \''..type(k)..'\'') end
	if type(v)=='function' or type(v)=='userdata' or type(v)=='thread' then error('Invalid value type \''..type(v)..'\'') end
	if type(v)=='table' then
		make_scoredata_table(v)
	end
	getmetatable(t).data[k]=v
	SaveScoreData()
end
function scoredata_mt_index(t,k)
	return getmetatable(t).data[k]
end
function make_scoredata_table(t)
	if type(t)~='table' then error('t must be a table') end
	Serialize(t)
	setmetatable(t,{__newindex=scoredata_mt_newindex,__index=scoredata_mt_index,data={}})
	for k,v in pairs(t) do
		if type(v)=='table' then
			make_scoredata_table(v)
		end
		getmetatable(t).data[k]=v
		t[k]=nil
	end
end

function DefineDefaultScoreData(t)
	scoredata=t
end

function SaveScoreData()
	local score_data_file=assert(io.open('score\\'..setting.mod..'\\'..setting.username..'.dat','w'))
	local s=Serialize(scoredata)
	score_data_file:write(s)
	score_data_file:close()
end

--restore all defined classes
all_class={}
class_name={}
--define new class
function Class(base, define)
	base=base or object
	if (type(base)~='table') or not base.is_class then
		error('Invalid base class or base class does not exist.')
	end
	local result={0,0,0,0,0,0}
	result.is_class=true
	result.init=base.init
	result.del=base.del
	result.frame=base.frame
	result.render=base.render
	result.colli=base.colli
	result.kill=base.kill
	result.base=base
	if define then
		for k,v in pairs(define) do
			result[k] = v
		end
	end
	table.insert(all_class,result)
	return result
end
--base class of all classes
object={0,0,0,0,0,0;
	is_class=true,
	init=function()end,
	del=function()end,
	frame=function()end,
	render=DefaultRenderFunc,
	colli=function(other)end,
	kill=function()end
}


--task
task={}
task.stack={}
function task:New(f)
	if not self.task then self.task={} end
	table.insert(self.task,coroutine.create(f))
end

function task:Do()
	if self.task then
		for _,co in pairs(self.task) do
			if coroutine.status(co)~='dead' then
				table.insert(task.stack,self)
				local _,errmsg=coroutine.resume(co)
				if errmsg then error(errmsg) end
				task.stack[#task.stack]=nil
			end
		end
	end
end

function task:Clear() self.task=nil end

function task.Wait(t)
	t=t or 1
	t=max(1,int(t))
	for i=1,t do coroutine.yield() end
end

function task.Until(t)
	t=int(t)
	while task.GetSelf().timer<t do
		coroutine.yield()
	end
end

function task.GetSelf() return task.stack[#task.stack] end

MOVE_NORMAL=0
MOVE_ACCEL=1
MOVE_DECEL=2
MOVE_ACC_DEC=3
function task.MoveTo(x,y,t,mode)
	local self=task.GetSelf()
	t=int(t)
	t=max(1,t)
	local dx=x-self.x
	local dy=y-self.y
	local xs=self.x
	local ys=self.y
	if mode==1 then
		for s=1/t,1+0.5/t,1/t do
			s=s*s
			self.x=xs+s*dx
			self.y=ys+s*dy
			coroutine.yield()
		end
	elseif mode==2 then
		for s=1/t,1+0.5/t,1/t do
			s=s*2-s*s
			self.x=xs+s*dx
			self.y=ys+s*dy
			coroutine.yield()
		end
	elseif mode==3 then
		for s=1/t,1+0.5/t,1/t do
			if s<0.5 then s=s*s*2 else s=-2*s*s+4*s-1 end
			self.x=xs+s*dx
			self.y=ys+s*dy
			coroutine.yield()
		end
	else
		for s=1/t,1+0.5/t,1/t do
			self.x=xs+s*dx
			self.y=ys+s*dy
			coroutine.yield()
		end
	end
end

function task.MoveToPlayer(t)
	local dirx,diry
	local self=task.GetSelf()
	if self.x>64 then dirx=-1 elseif self.x<-64 then dirx=1
	else
		if self.x>lstg.player.x then dirx=-1 else dirx=1 end
	end
	if self.y>144 then diry=-1 elseif self.y<128 then diry=1 else diry=ran:Sign() end
	local dx=max(16,min(abs((self.x-lstg.player.x)*0.3),32))
	task.MoveTo(self.x+ran:Float(dx,dx*2)*dirx,self.y+diry*ran:Float(16,32),t)
end

stage={stages={}}
function stage.init(self) end
function stage.del(self) end
function stage.render(self) end
function stage.frame(self) end
function stage.New(stage_name,as_entrance,is_menu)
	local result={init=stage.init,
			del=stage.del,
			render=stage.render,
			frame=stage.frame,
			}
	if as_entrance then stage.next_stage=result end
	result.is_menu=is_menu
	result.stage_name=tostring(stage_name)
	stage.stages[stage_name]=result
	return result
end

function stage.Set(stage_name)
	stage.next_stage=stage.stages[stage_name]
	KeyStatePre={}
end

function stage.SetTimer(t)
	stage.current_stage.timer=t-1
end

function stage.QuitGame()
	lstg.quit_flag=true
end

KeyState={}
KeyStatePre={}
function GetInput()
	for k,v in pairs(setting.keys) do
		KeyStatePre[k]=KeyState[k]
		KeyState[k]=GetKeyState(v)
	end
end
function KeyIsDown(key)
	return KeyState[key]
end
function KeyIsPressed(key)
	return KeyState[key] and (not KeyStatePre[key])
end
KeyPress = KeyIsDown
KeyTrigger = KeyIsPressed

lstg.quit_flag=false
lstg.paused=false

lstg.var={username=setting.username}
lstg.tmpvar={}

function SetGlobal(k,v) lstg.var[k]=v end
function GetGlobal(k,v) return lstg.var[k] end

Color = UnityEngine.Color;

lstg.view3d={}
lstg.view3d.eye={0,0,-1}
lstg.view3d.at={0,0,0}
lstg.view3d.up={0,1,0}
lstg.view3d.fovy=PI_2
lstg.view3d.z={0,2}
lstg.view3d.fog={0,0,Color(0x00000000)}

function Set3D(key,a,b,c)
	if key=='fog' then
		a=tonumber(a or 0)
		b=tonumber(b or 0)
		lstg.view3d.fog={a,b,c}
		return
	end
	a=tonumber(a or 0)
	b=tonumber(b or 0)
	c=tonumber(c or 0)
	if key=='eye' then lstg.view3d.eye={a,b,c}
	elseif key=='at' then lstg.view3d.at={a,b,c}
	elseif key=='up' then lstg.view3d.up={a,b,c}
	elseif key=='fovy' then lstg.view3d.fovy=a
	elseif key=='z' then lstg.view3d.z={a,b}
	end
end

function Reset3D()
	lstg.view3d.eye={0,0,-1}
	lstg.view3d.at={0,0,0}
	lstg.view3d.up={0,1,0}
	lstg.view3d.fovy=PI_2
	lstg.view3d.z={1,2}
	lstg.view3d.fog={0,0,Color(0x00000000)}
end

function SetViewMode(mode)
	lstg.viewmode=mode
	if mode=='3d' then
		SetViewport(lstg.world.scrl*screen.scale+screen.dx,lstg.world.scrr*screen.scale+screen.dx,lstg.world.scrb*screen.scale+screen.dy,lstg.world.scrt*screen.scale+screen.dy)
		SetPerspective(lstg.view3d.eye[1],lstg.view3d.eye[2],lstg.view3d.eye[3],
					   lstg.view3d.at[1],lstg.view3d.at[2],lstg.view3d.at[3],
					   lstg.view3d.up[1],lstg.view3d.up[2],lstg.view3d.up[3],
					   lstg.view3d.fovy,(lstg.world.r-lstg.world.l)/(lstg.world.t-lstg.world.b),
					   lstg.view3d.z[1],lstg.view3d.z[2])
		SetFog(lstg.view3d.fog[1],lstg.view3d.fog[2],lstg.view3d.fog[3])
		SetImageScale(((((lstg.view3d.eye[1]-lstg.view3d.at[1])^2+(lstg.view3d.eye[2]-lstg.view3d.at[2])^2+(lstg.view3d.eye[3]-lstg.view3d.at[3])^2)^0.5)*2*math.tan(lstg.view3d.fovy*0.5))/(lstg.world.scrr-lstg.world.scrl))
	elseif mode=='world' then
		SetViewport(lstg.world.scrl*screen.scale+screen.dx,lstg.world.scrr*screen.scale+screen.dx,lstg.world.scrb*screen.scale+screen.dy,lstg.world.scrt*screen.scale+screen.dy)
		SetOrtho(lstg.world.l,lstg.world.r,lstg.world.b,lstg.world.t)
		SetFog()
		SetImageScale((lstg.world.r-lstg.world.l)/(lstg.world.scrr-lstg.world.scrl))
	elseif mode=='ui' then
		SetOrtho(0.5,screen.width+0.5,-0.5,screen.height-0.5)
		SetViewport(screen.dx,screen.width*screen.scale+screen.dx,screen.dy,screen.height*screen.scale+screen.dy)
		SetFog()
		SetImageScale(1)
	else error('Invalid arguement.') end
end

function BeforeRender() end
function AfterRender() end

function UserSystemOperation()
	--assistance of Polar coordinate system
	local polar,radius,angle,delta,omiga,center
	for id = 0,15 do
		for _,obj in ObjList(id) do
			polar=obj.polar
			if polar then
				radius = polar.radius or 0
				angle = polar.angle or 0
				delta = polar.delta
				if delta then polar.radius = radius + delta end
				omiga = polar.omiga
				if omiga then polar.angle = angle + omiga end
				center = polar.center or {x=0,y=0}
				radius = polar.radius
				angle = polar.angle
				obj.x = center.x + radius * cos(angle)
				obj.y = center.y + radius * sin(angle)
			end
		end
	end
	--acceleration and gravity
	local alist,accelx,accely,gravity
	for id = 0,15 do
		for _,obj in ObjList(id) do
			alist = obj.acceleration
			if alist then
				accelx = alist.ax
				if accelx then obj.vx = obj.vx + accelx end
				accely = alist.ay
				if accely then obj.vy = obj.vy + accely end
				gravity = alist.g
				if gravity then obj.vy = obj.vy - gravity end
			end
		end
	end
	--limitation of velocity
	local forbid,vx,vy,v,ovx,ovy,cache
	for id = 0,15 do
		for _,obj in ObjList(id) do
			forbid = obj.forbidveloc
			if forbid then
				ovx = obj.vx
				ovy = obj.vy
				v = forbid.v
				if v and (v*v) < (ovx*ovx+ovy*ovy) then
					cache = v/hypot(ovx,ovy)
					obj.vx = cache*ovx
					obj.vy = cache*ovy
					ovx = obj.vx
					ovy = obj.vy
				end
				vx = forbid.vx
				vy = forbid.vy
				if vx and vx < abs(ovx) then obj.vx = vx end
				if vy and vy < abs(ovy) then obj.vy = vy end
			end
		end
	end
end

Timer = 0

Foo=Class(object)

function Foo:frame()
	self.x = 5 * lstg.cos(self.timer);
	self.y = 5 * lstg.sin(self.timer);
end

function Foo:colli()
	lstg.Kill(self);
end

Bar=Class(object)

function FrameFunc()
	Timer = Timer + 1
	if Timer <= 50 then
		--lstg.Print("FrameFunc:", lstg.GetFPS());
		local obj = lstg.New(Foo);
		obj.img = "undefined";
		obj.group = GROUP_ENEMY_BULLET;
		obj.x = 5;
		obj.omiga = 1;
		--obj.vx = 1;
	end
	return false
end

function RenderFunc()
	lstg.Print("RenderFunc");
end

function FocusLoseFunc()
	lstg.Print("FocusLoseFunc");
end

function FocusGainFunc()
	lstg.Print("FocusGainFunc");
end

function GameInit()
	lstg.Print("GameInit");
	lstg.LoadTexture("undefinedTex", "undefined.png");
	lstg.LoadImage("undefined", "undefinedTex", 0, 0, 128, 128)
	lstg.SetBound(-5, 5, -5, 5);
	lstg.LoadPS('graze','graze.psi','undefined');
	CreateUI("test", "test.json");
	--local obj = lstg.New(Bar);
	--obj.img = "graze";
end

function GameExit()
	lstg.Print("GameExit");
end

for _,v in pairs(all_class) do
	v[1]=v.init;
	v[2]=v.del;
	v[3]=v.frame;
	v[4]=v.render;
	v[5]=v.colli;
	v[6]=v.kill;
end
