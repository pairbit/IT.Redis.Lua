if redis.call('exists', KEYS[1]) == 1 then
	if redis.call('hexists', KEYS[1], ARGV[1]) ~= 1 then
		return redis.call('hset', KEYS[1], unpack(ARGV, 2))
	else
		return -2
	end
else
	return -1
end