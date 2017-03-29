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

lstg.quit_flag=false
lstg.paused=false

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

Timer = 0

Foo=Class(object)

function Foo:frame()
	self.x = 5 * lstg.cos(self.timer);
	self.y = 5 * lstg.sin(self.timer);
end

function Foo:colli()
	lstg.Kill(self);
end

function FrameFunc()
	Timer = Timer + 1
	if Timer <= 200 then
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
end

function GameExit()
	lstg.Print("GameExit");
end

for _,v in pairs(all_class) do
	v[1]=v.init
	v[2]=v.del
	v[3]=v.frame
	v[4]=v.render
	v[5]=v.colli
	v[6]=v.kill
end
