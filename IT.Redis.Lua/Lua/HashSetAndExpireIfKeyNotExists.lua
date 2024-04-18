if redis.call('exists', KEYS[1]) ~= 1 then
	local val = redis.call('hset', KEYS[1], unpack(ARGV, 2))
	redis.call('pexpireat', KEYS[1], ARGV[1])
	return val
else
	return -1
end