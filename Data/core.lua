GROUP_GHOST=0
GROUP_ENEMY_BULLET=1
GROUP_ENEMY=2
GROUP_PLAYER_BULLET=3
GROUP_PLAYER=4
GROUP_INDES=5
GROUP_ITEM=6
GROUP_NONTJT = 7
GROUP_ALL=16
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
	lstg.Print(self.rot);
end

function FrameFunc()
	Timer = Timer + 1
	if Timer % 60 == 0 then
		lstg.Print("FrameFunc:", lstg.GetFPS())
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
	lstg.SetBound(-10, 10, -10, 10);
	local obj = lstg.New(Foo);
	lstg.Print(obj.x, obj.img == nil);
	obj.img = "undefined";
	obj.vx = 0.05;
	obj.omiga = 1;
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
