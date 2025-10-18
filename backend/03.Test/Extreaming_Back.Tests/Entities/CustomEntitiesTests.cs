using Core.CustomEntities;
using Domain.CustomEntities;
using Xunit;
using System.Collections.Generic;

public class CustomEntitiesTests
{
    [Fact]
    public void ResponseMessage_DefaultCtor_InitializesDictionary()
    {
        var rm = new ResponseMessage();
        Assert.NotNull(rm.Values);
        Assert.Empty(rm.Values);
    }

    [Fact]
    public void ResponseMessage_WithDictionary_StoresReference()
    {
        var dict = new Dictionary<string, string>{{"a","b"}};
        var rm = new ResponseMessage(dict);
        Assert.Equal(dict, rm.Values);
    }

    [Fact]
    public void CustomResponseResult_BuildsResponse()
    {
        var result = new CustomResponseResult(200, "ok", new {x=1});
        var resp = Assert.IsType<Response>(result.Value);
        Assert.Equal(200, resp.Status);
        Assert.Equal("ok", resp.Message);
    }

    [Fact]
    public void DataMessage_PropertyRoundTrip()
    {
        var msg = new DataMessage { SourceIp="1", SourceEntity="2" };
        Assert.Equal("1", msg.SourceIp);
        Assert.Equal("2", msg.SourceEntity);
    }

    [Fact]
    public void Response_PropertyRoundTrip()
    {
        var resp = new Response { Status = 200, Message = "ok", Description = "d" };
        Assert.Equal(200, resp.Status);
        Assert.Equal("ok", resp.Message);
        Assert.Equal("d", resp.Description);
    }
}
