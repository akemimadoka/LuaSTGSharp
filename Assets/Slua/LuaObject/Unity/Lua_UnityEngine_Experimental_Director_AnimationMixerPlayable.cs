﻿using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Experimental_Director_AnimationMixerPlayable : LuaObject {
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int constructor(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable o;
			o=new UnityEngine.Experimental.Director.AnimationMixerPlayable();
			pushValue(l,true);
			pushValue(l,o);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int Destroy(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			self.Destroy();
			pushValue(l,true);
			setBack(l,self);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetInput(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			System.Int32 a1;
			checkType(l,2,out a1);
			var ret=self.GetInput(a1);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetOutput(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			System.Int32 a1;
			checkType(l,2,out a1);
			var ret=self.GetOutput(a1);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetInputs(IntPtr l) {
		try {
			int argc = LuaDLL.lua_gettop(l);
			if(matchType(l,argc,2,typeof(IEnumerable<UnityEngine.Experimental.Director.Playable>))){
				UnityEngine.Experimental.Director.AnimationMixerPlayable self;
				checkValueType(l,1,out self);
				System.Collections.Generic.IEnumerable<UnityEngine.Experimental.Director.Playable> a1;
				checkType(l,2,out a1);
				var ret=self.SetInputs(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			else if(matchType(l,argc,2,typeof(UnityEngine.AnimationClip[]))){
				UnityEngine.Experimental.Director.AnimationMixerPlayable self;
				checkValueType(l,1,out self);
				UnityEngine.AnimationClip[] a1;
				checkArray(l,2,out a1);
				var ret=self.SetInputs(a1);
				pushValue(l,true);
				pushValue(l,ret);
				return 2;
			}
			pushValue(l,false);
			LuaDLL.lua_pushstring(l,"No matched override function to call");
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetInputWeight(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			System.Int32 a1;
			checkType(l,2,out a1);
			var ret=self.GetInputWeight(a1);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetInputWeight(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			System.Int32 a1;
			checkType(l,2,out a1);
			System.Single a2;
			checkType(l,3,out a2);
			self.SetInputWeight(a1,a2);
			pushValue(l,true);
			setBack(l,self);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int IsValid(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			var ret=self.IsValid();
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int AddInput(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			UnityEngine.Experimental.Director.Playable a1;
			checkValueType(l,2,out a1);
			var ret=self.AddInput(a1);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetInput(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			UnityEngine.Experimental.Director.Playable a1;
			checkValueType(l,2,out a1);
			System.Int32 a2;
			checkType(l,3,out a2);
			var ret=self.SetInput(a1,a2);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int RemoveInput(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			System.Int32 a1;
			checkType(l,2,out a1);
			var ret=self.RemoveInput(a1);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int RemoveAllInputs(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			var ret=self.RemoveAllInputs();
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int Create_s(IntPtr l) {
		try {
			var ret=UnityEngine.Experimental.Director.AnimationMixerPlayable.Create();
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int op_Equality(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable a1;
			checkValueType(l,1,out a1);
			UnityEngine.Experimental.Director.Playable a2;
			checkValueType(l,2,out a2);
			var ret=(a1==a2);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int op_Inequality(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable a1;
			checkValueType(l,1,out a1);
			UnityEngine.Experimental.Director.Playable a2;
			checkValueType(l,2,out a2);
			var ret=(a1!=a2);
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_inputCount(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			pushValue(l,true);
			pushValue(l,self.inputCount);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_outputCount(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			pushValue(l,true);
			pushValue(l,self.outputCount);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_state(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			pushValue(l,true);
			pushEnum(l,(int)self.state);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_state(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			UnityEngine.Experimental.Director.PlayState v;
			checkEnum(l,2,out v);
			self.state=v;
			setBack(l,self);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_time(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			pushValue(l,true);
			pushValue(l,self.time);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_time(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			double v;
			checkType(l,2,out v);
			self.time=v;
			setBack(l,self);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int get_duration(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			pushValue(l,true);
			pushValue(l,self.duration);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int set_duration(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.AnimationMixerPlayable self;
			checkValueType(l,1,out self);
			double v;
			checkType(l,2,out v);
			self.duration=v;
			setBack(l,self);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.Experimental.Director.AnimationMixerPlayable");
		addMember(l,Destroy);
		addMember(l,GetInput);
		addMember(l,GetOutput);
		addMember(l,SetInputs);
		addMember(l,GetInputWeight);
		addMember(l,SetInputWeight);
		addMember(l,IsValid);
		addMember(l,AddInput);
		addMember(l,SetInput);
		addMember(l,RemoveInput);
		addMember(l,RemoveAllInputs);
		addMember(l,Create_s);
		addMember(l,op_Equality);
		addMember(l,op_Inequality);
		addMember(l,"inputCount",get_inputCount,null,true);
		addMember(l,"outputCount",get_outputCount,null,true);
		addMember(l,"state",get_state,set_state,true);
		addMember(l,"time",get_time,set_time,true);
		addMember(l,"duration",get_duration,set_duration,true);
		createTypeMetatable(l,constructor, typeof(UnityEngine.Experimental.Director.AnimationMixerPlayable),typeof(System.ValueType));
	}
}
